using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using Randstad.UfoRsm.BabelFish.Translators;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class TimesheetTranslation : TranslatorBase, ITranslator
    {

        private readonly string _baseRoutingKey;
        private readonly Dictionary<string, string> _rateCodes;

        public TimesheetTranslation(IProducerService producer, string baseRoutingKey, ILogger logger, Dictionary<string, string> rateCodes, string opCosToSend, bool allowBlockByDivision) : base(producer, baseRoutingKey, logger, opCosToSend, allowBlockByDivision)
        {
            _baseRoutingKey = baseRoutingKey;
            _rateCodes = rateCodes;

        }

        public async Task Translate(ExportedEntity entity)
        {

            if (entity.ObjectType != "Timesheet") return;

            Timesheet timesheet = null;
            try
            {
                timesheet = JsonConvert.DeserializeObject<Timesheet>(entity.Payload);

                if (BlockExport(Mappers.MapOpCoFromName(timesheet.OpCo.Name)))
                {
                    _logger.Warn($"Timesheet OpCo not live in RSM {timesheet.OpCo.Name} {timesheet.TimesheetRef}", entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (BlockExportByDivision(timesheet.Division.Name))
                {
                    _logger.Warn($"Timesheet Division not live in RSM {timesheet.TimesheetRef} {timesheet.Division.Name}", entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if ((timesheet.TimesheetLines==null || !timesheet.TimesheetLines.Any()) && (timesheet.Expenses==null || !timesheet.Expenses.Any()))
                {
                    _logger.Warn($"No Timesheetlines or expenses on {timesheet.TimesheetRef} Opco", entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (timesheet.TimesheetLines != null && !timesheet.TimesheetLines.Any())
                {
                    var noRate = timesheet.TimesheetLines.Where(x => x.Rate == null);

                    if (noRate.Any())
                    {
                        _logger.Warn($"Timesheet contains lines with no rates on {timesheet.TimesheetRef}",
                            entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                        entity.ExportSuccess = false;
                        return;
                    }
                }

                //var timesheetsNoRates = timesheet.TimesheetLines.Where(x => x.Rate == null);

                //if (timesheetsNoRates.Any())
                //{
                //    _logger.Warn($"Timesheet {timesheet.TimesheetRef} contains lines with no rates", entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                //    entity.ExportSuccess = false;
                //    return;
                //}


            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising Timesheet from UFO {entity.ObjectId} - {exp.Message}");
            }

            _logger.Success($"Received Timesheet {timesheet.TimesheetRef}", entity.CorrelationId, timesheet, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null);

            RSM.ExpenseClaim claim = null;
            List<RSM.Timesheet> mappedTimesheetList = null;


            try
            {
                mappedTimesheetList = timesheet.MapTimesheet(_logger, _rateCodes, entity.CorrelationId, out claim);
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem mapping Timesheet from UFO {entity.ObjectId} - {exp.Message}");
            }

            foreach (var ts in mappedTimesheetList)
            {
                if (timesheet.TimesheetRef.StartsWith("UT"))
                {
                    SendToRsm(JsonConvert.SerializeObject(ts), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "Timesheet", entity.CorrelationId, true);

                    _logger.Success($"Successfully mapped Timesheet {timesheet.TimesheetRef} and sent to RSM",
                        entity.CorrelationId, ts, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,
                        "RSM.Timesheet");
                }

                if (timesheet.TimesheetRef.StartsWith("NT"))
                {
                    SendToRsm(JsonConvert.SerializeObject(ts), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "Timesheet", entity.CorrelationId, true, true);

                    _logger.Success($"Successfully mapped Netive Timesheet {timesheet.TimesheetRef} and sent to Adjustment Service",
                        entity.CorrelationId, ts, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,
                        "RSM.Timesheet");
                }
            }

            if (claim != null && claim.expenseItems!=null && claim.expenseItems.Any())
            {
                if (timesheet.TimesheetRef.StartsWith("UT"))
                {
                    SendToRsm(JsonConvert.SerializeObject(claim), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "ExpenseClaim", entity.CorrelationId, true);
                    _logger.Success($"Successfully mapped expenses for {timesheet.TimesheetRef} and sent to RSM",
                        entity.CorrelationId, claim, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,
                        "RSM.ExpenseClaim");
                }

                if (timesheet.TimesheetRef.StartsWith("NT"))
                {
                    SendToRsm(JsonConvert.SerializeObject(claim), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "ExpenseClaim", entity.CorrelationId, true, true);
                    _logger.Success($"Successfully mapped expenses for {timesheet.TimesheetRef} and sent to Adjustment Service", entity.CorrelationId, claim, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,"RSM.ExpenseClaim");
                }
            }

            entity.ExportSuccess = true;
        }
    }
    
}
