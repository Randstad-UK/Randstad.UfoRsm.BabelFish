using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoSti.BabelFish.Dtos.Ufo;
using Randstad.UfoSti.BabelFish.Helpers;
using RandstadMessageExchange;

namespace Randstad.UfoSti.BabelFish.Translators
{
    public class TimesheetTranslation : TranslatorBase, ITranslator
    {
        private readonly Dictionary<string, string> _rateCodes;
        private readonly string _baseRoutingKey;
        private readonly string _consultantCodePrefix;
        private readonly Dictionary<string, string> _tomCodes;
        private readonly Dictionary<string, string> _employerRefs;

        public TimesheetTranslation(Dictionary<string, string> rateCodes, IProducerService producer, string baseRoutingKey, string consultantCodePrefix, Dictionary<string, string> tomCodes, ILogger logger, Dictionary<string, string> employerRefs) : base(producer, baseRoutingKey, logger)
        {
            _rateCodes = rateCodes;
            _baseRoutingKey = baseRoutingKey;
            _consultantCodePrefix = consultantCodePrefix;
            _tomCodes = tomCodes;
            _employerRefs = employerRefs;
        }

        public async Task Translate(ExportedEntity entity)
        {

            if (entity.ObjectType != "Timesheet") return;

            Timesheet timesheet = null;
            try
            {
                timesheet = JsonConvert.DeserializeObject<Timesheet>(entity.Payload);

                if (string.IsNullOrEmpty(timesheet.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {timesheet.TimesheetRef} Opco", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if ((timesheet.TimesheetLines==null || !timesheet.TimesheetLines.Any()) && (timesheet.Expenses==null || !timesheet.Expenses.Any()))
                {
                    _logger.Warn($"No Timesheetlines or expenses on {timesheet.TimesheetRef} Opco", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (timesheet.TimesheetLines != null && !timesheet.TimesheetLines.Any())
                {
                    var noRate = timesheet.TimesheetLines.Where(x => x.Rate == null);

                    if (noRate.Any())
                    {
                        _logger.Warn($"Timesheet contains lines with no rates on {timesheet.TimesheetRef}",
                            entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
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

            _logger.Success($"Recieved Timesheet {timesheet.TimesheetRef}", entity.CorrelationId, timesheet, entity.ObjectId, "Dtos.Ufo.Timesheet", null);

            

            List<Dtos.Sti.Timesheet> mappedTimesheetList = null;

            try
            {
                mappedTimesheetList = timesheet.MapTimesheet(_rateCodes, _consultantCodePrefix, _tomCodes, _employerRefs);
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map timesheet {timesheet.TimesheetRef}: {exp.Message}", entity.CorrelationId, timesheet, entity.ObjectId, "Dtos.Ufo.Timesheet", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }

            //if (!mappedTimesheetList.Any())
            //{
            //    _logger.Warn($"Failed to map timesheet {timesheet.TimesheetRef}: {exp.Message}", entity.CorrelationId, timesheet, entity.ObjectId, "Dtos.Ufo.Timesheet", null);
            //}

            foreach (var ts in mappedTimesheetList)
            {
                SendToSti(JsonConvert.SerializeObject(ts), ts.OpCo.ToString(), "Timesheet", entity.CorrelationId, true);
                _logger.Success($"Successfully mapped Timesheet {ts.TimesheetNumber} and sent to STI", entity.CorrelationId, timesheet, timesheet.TimesheetId, "Dtos.Ufo.Timesheet", null, ts, "Dtos.Sti.Timesheet");
            }

            entity.ExportSuccess = true;
        }
    }
    
}
