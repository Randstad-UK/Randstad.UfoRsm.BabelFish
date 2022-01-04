using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoSti.BabelFish.Dtos.Sti;
using Randstad.UfoSti.BabelFish.Dtos.Ufo;
using Randstad.UfoSti.BabelFish.Helpers;
using RandstadMessageExchange;
using Consultant = Randstad.UfoSti.BabelFish.Dtos.Ufo.Consultant;

namespace Randstad.UfoSti.BabelFish.Translators
{
    public class HolidayRequestTranslation : TranslatorBase, ITranslator
    {
        private readonly Dictionary<string, string> _employerRefs;

        public HolidayRequestTranslation(IProducerService producer, string routingKeyBase, Dictionary<string, string> employerRefs, ILogger logger) : base(producer, routingKeyBase, logger)
        {
            _employerRefs = employerRefs;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "HolidayRequest") return;

            Dtos.Ufo.HolidayRequest holidayRequest = null;
            
            try
            {
                holidayRequest = JsonConvert.DeserializeObject<Dtos.Ufo.HolidayRequest>(entity.Payload);


                if (string.IsNullOrEmpty(holidayRequest.Candidate.OperatingCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {holidayRequest.Candidate.CandidateRef} Opco", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Holiday Request from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved HolidayRequest {holidayRequest.EmployerRef}", entity.CorrelationId,
                holidayRequest, entity.ObjectId, "Dtos.Ufo.HolidayRequest", null);


            Dtos.Sti.HolidayRequest stiHolidayRequest = null;
            try
            {
                stiHolidayRequest = holidayRequest.MapHolidayRequest(_employerRefs);
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map holiday request {holidayRequest.Candidate.CandidateRef}: {exp.Message}",
                    entity.CorrelationId, holidayRequest, entity.ObjectId, "Dtos.Ufo.HolidayRequest", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }


            SendToSti(JsonConvert.SerializeObject(stiHolidayRequest), Mappers.MapOpCo(holidayRequest.Candidate.OperatingCo.FinanceCode).ToString(), "holidayrequest", entity.CorrelationId, true);

            _logger.Success($"Successfully sent holidayrequest {holidayRequest.WorkerRef} to {holidayRequest.Candidate.OperatingCo.FinanceCode} on {_updatedRoutingKey}", entity.CorrelationId,
                holidayRequest, entity.ObjectId, "Dtos.Ufo.HolidayRequest", null, stiHolidayRequest, "Dtos.Sti.HolidayRequest");



            entity.ExportSuccess = true;
        }
    }
}
