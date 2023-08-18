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
using Randstad.UfRsm.BabelFish.Dtos.Ufo;
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

                _logger.Success($"Received Timesheet {timesheet.TimesheetRef}", entity.CorrelationId, timesheet, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null);

                if (timesheet.OpCo.Name.ToLower().Contains("pareto"))
                {
                    _logger.Warn($"Pareto Timesheets not live in RSM {timesheet.TimesheetRef}", entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

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

                if (string.IsNullOrEmpty(timesheet.ApprovedBy))
                {
                    _logger.Warn($"Timesheet {timesheet.TimesheetRef} is not approved", entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (!timesheet.Cancelled && (timesheet.TimesheetLines == null || !timesheet.TimesheetLines.Any()) && (timesheet.Expenses == null || !timesheet.Expenses.Any()))
                {
                    _logger.Warn($"No Timesheetlines or expenses on {timesheet.TimesheetRef} Opco", entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (timesheet.TimesheetLines != null && timesheet.TimesheetLines.Any())
                {
                    var noRate = timesheet.TimesheetLines.Where(x => x.Rate == null);

                    if (noRate.Any())
                    {
                        _logger.Warn($"Timesheet contains lines with no rates on {timesheet.TimesheetRef}",
                            entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);
                        entity.ExportSuccess = false;
                        return;
                    }

                    //var zeroDays = timesheet.TimesheetLines.Where(x => x.DaysReported == 0 && x.TotalHours!=null);

                    //if (zeroDays.Any())
                    //{
                    //    _logger.Warn($"Timesheet contains lines with 0 in the days reported {timesheet.TimesheetRef}",
                    //        entity.CorrelationId, entity, timesheet.TimesheetRef, "Dtos.Ufo.ExportedEntity", null);

                    //    foreach(var cl in zeroDays)
                    //    {
                    //        cl.DaysReported = null;
                    //    }
                    //}
                }

            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising Timesheet from UFO {entity.ObjectId} - {exp.Message}");
            }



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
                //None Netive timesheets go directly to Back Office
                if (!timesheet.TimesheetRef.StartsWith("NT"))
                {
                    if (timesheet.Division.Name == "Tuition Services" || timesheet.Division.Name == "Student Support")
                    {
                        SendToRsm(JsonConvert.SerializeObject(ts), "sws", "Timesheet", entity.CorrelationId, true, false, "Create");

                        _logger.Success($"Successfully mapped Timesheet {timesheet.TimesheetRef} and sent to SWS RSM",
                            entity.CorrelationId, ts, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,
                            "RSM.Timesheet");
                    }
                    else
                    {
                        SendToRsm(JsonConvert.SerializeObject(ts), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "Timesheet", entity.CorrelationId, true, false, "Create");

                        _logger.Success($"Successfully mapped Timesheet {timesheet.TimesheetRef} and sent to RSM",
                            entity.CorrelationId, ts, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,
                            "RSM.Timesheet");
                    }


                }

                //Netive timesheets go to the adjustment service
                if (timesheet.TimesheetRef.StartsWith("NT"))
                {
                    SendToRsm(JsonConvert.SerializeObject(ts), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "Timesheet", entity.CorrelationId, true, true);

                    _logger.Success($"Successfully mapped Netive Timesheet {timesheet.TimesheetRef} and sent to Adjustment Service",
                        entity.CorrelationId, ts, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,
                        "RSM.Timesheet");
                }
            }

            if(!mappedTimesheetList.Any() && timesheet.TimesheetLines == null)
            {
                _logger.Warn($"Timesheet {timesheet.TimesheetRef} has clock entries but these were not valid for RSM e.g. contained 0 hours and 0 days", entity.CorrelationId, timesheet, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null);
            }

            if (claim != null && claim.expenseItems != null && claim.expenseItems.Any())
            {
                //None Netive timesheets go directly to Back Office
                if (!timesheet.TimesheetRef.StartsWith("NT"))
                {
                    if (timesheet.Division.Name == "Tuition Services" || timesheet.Division.Name == "Student Support")
                    {
                        SendToRsm(JsonConvert.SerializeObject(claim), "sws", "ExpenseClaim", entity.CorrelationId, true, false, "Create");
                        _logger.Success($"Successfully mapped expenses for {timesheet.TimesheetRef} and sent to SWS RSM",
                            entity.CorrelationId, claim, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,
                            "RSM.ExpenseClaim");
                    }
                    else
                    {
                        SendToRsm(JsonConvert.SerializeObject(claim), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "ExpenseClaim", entity.CorrelationId, true, false, "Create");
                        _logger.Success($"Successfully mapped expenses for {timesheet.TimesheetRef} and sent to RSM",
                            entity.CorrelationId, claim, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null,
                            "RSM.ExpenseClaim");
                    }


                }

                //Netive timesheets go directly to 
                if (timesheet.TimesheetRef.StartsWith("NT"))
                {
                    SendToRsm(JsonConvert.SerializeObject(claim), Mappers.MapOpCoFromName(timesheet.OpCo.Name.ToLower()).ToString(), "ExpenseClaim", entity.CorrelationId, true, true, entity.EventType);
                    _logger.Success($"Successfully mapped expenses for {timesheet.TimesheetRef} and sent to Adjustment Service", entity.CorrelationId, claim, timesheet.TimesheetRef, "Dtos.Ufo.Timesheet", null, null, "RSM.ExpenseClaim");
                }
            }

            entity.ExportSuccess = true;
        }
    }

}
