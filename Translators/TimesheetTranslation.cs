using System;
using System.Collections.Generic;
using System.Linq;
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

        public TimesheetTranslation(IProducerService producer, string baseRoutingKey, ILogger logger, Dictionary<string, string> rateCodes) : base(producer, baseRoutingKey, logger)
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

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Timesheet from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Timesheet {timesheet.TimesheetRef}", entity.CorrelationId, timesheet, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null);

            RSM.ExpenseClaim claim = null;
            List<RSM.Timesheet> mappedTimesheetList = null;


            try
            {
                mappedTimesheetList = timesheet.MapTimesheet(_logger, _rateCodes, entity.CorrelationId, out claim);
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map timesheet {timesheet.TimesheetRef}: {exp.Message}", entity.CorrelationId, timesheet, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }

            foreach (var ts in mappedTimesheetList)
            {
                SendToRsm(JsonConvert.SerializeObject(ts), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "Timesheet", entity.CorrelationId, true);
                _logger.Success($"Successfully mapped Timesheet {timesheet.TimesheetRef} and sent to RSM", entity.CorrelationId, timesheet, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, ts, "RSM.Timesheet");
            }

            if (claim != null)
            {
                SendToRsm(JsonConvert.SerializeObject(claim), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "ExpenseClaim", entity.CorrelationId, true);
                _logger.Success($"Successfully mapped expenses for {timesheet.TimesheetRef} and sent to RSM", entity.CorrelationId, claim, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, claim, "RSM.ExpenseClaim");
            }

            entity.ExportSuccess = true;
        }
    }
    
}
