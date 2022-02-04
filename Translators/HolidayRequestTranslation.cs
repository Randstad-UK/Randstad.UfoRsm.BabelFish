using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos.RsmInherited;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using Randstad.UfoRsm.BabelFish.Translators;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class HolidayRequestTranslation : TranslatorBase, ITranslator
    {

        public HolidayRequestTranslation(IProducerService producer, string routingKeyBase, ILogger logger) : base(producer, routingKeyBase, logger)
        {

        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "HolidayRequest") return;

            Randstad.UfRsm.BabelFish.Dtos.Ufo.HolidayRequest holidayRequest = null;
            
            try
            {
                holidayRequest = JsonConvert.DeserializeObject<Randstad.UfRsm.BabelFish.Dtos.Ufo.HolidayRequest>(entity.Payload);
            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Holiday Request from UFO {exp.Message}", entity.CorrelationId, entity, entity.Id, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved HolidayRequest {holidayRequest.HolidayRequestRef}", entity.CorrelationId,
                holidayRequest, holidayRequest.HolidayRequestRef, "Dtos.Ufo.HolidayRequest", null);


            HolidayRequest rsmHolidayRequest = null;
            try
            {
                rsmHolidayRequest = holidayRequest.MapHolidayRequest();
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map holiday request {holidayRequest.Candidate.CandidateRef}: {exp.Message}", entity.CorrelationId, holidayRequest, holidayRequest.HolidayRequestRef, "Dtos.Ufo.HolidayRequest", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }


            SendToRsm(JsonConvert.SerializeObject(rsmHolidayRequest), Mappers.MapOpCoFromName(holidayRequest.Candidate.OperatingCo.FinanceCode).ToString(), "holidayrequest", entity.CorrelationId, true);

            _logger.Success($"Successfully sent holidayrequest {holidayRequest.Candidate.CandidateRef} to {holidayRequest.Candidate.OperatingCo.FinanceCode} on {_updatedRoutingKey}", entity.CorrelationId,
                holidayRequest, holidayRequest.HolidayRequestRef, "Dtos.Ufo.HolidayRequest", null, rsmHolidayRequest, "Ufo.HolidayRequest");



            entity.ExportSuccess = true;
        }
    }
}
