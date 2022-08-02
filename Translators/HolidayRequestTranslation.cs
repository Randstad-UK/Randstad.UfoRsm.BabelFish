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

        public HolidayRequestTranslation(IProducerService producer, string routingKeyBase, ILogger logger, string opCosToSend, bool allowBlockByDivision) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
        {

        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "HolidayRequest") return;

            Randstad.UfRsm.BabelFish.Dtos.Ufo.HolidayRequest holidayRequest = null;
            
            try
            {
                holidayRequest = JsonConvert.DeserializeObject<Randstad.UfRsm.BabelFish.Dtos.Ufo.HolidayRequest>(entity.Payload);

                if (BlockExport(Mappers.MapOpCoFromName(holidayRequest.Candidate.OperatingCo.Name)))
                {
                    _logger.Warn($"Candidate OpCo not live in RSM {holidayRequest.Candidate.CandidateRef} {holidayRequest.Candidate.OperatingCo.Name}", entity.CorrelationId, entity, holidayRequest.HolidayRequestRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (BlockExportByDivision(holidayRequest.Candidate.Division.Name))
                {
                    _logger.Warn($"Candidate Division not live in RSM {holidayRequest.Candidate.CandidateRef} {holidayRequest.Candidate.Division.Name}", entity.CorrelationId, entity, holidayRequest.Candidate.CandidateRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising Holiday Request from UFO {entity.ObjectId} - {exp.Message}");
            }

            _logger.Success($"Received HolidayRequest {holidayRequest.HolidayRequestRef}", entity.CorrelationId,
                holidayRequest, holidayRequest.HolidayRequestRef, "Dtos.Ufo.HolidayRequest", null);


            Absence rsmHolidayRequest = null;
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


            SendToRsm(JsonConvert.SerializeObject(rsmHolidayRequest), Mappers.MapOpCoFromName(holidayRequest.Candidate.OperatingCo.Name).ToString(), "absence", entity.CorrelationId, true);

            _logger.Success($"Successfully sent holidayrequest {holidayRequest.Candidate.CandidateRef} to {holidayRequest.Candidate.OperatingCo.Name} on {_updatedRoutingKey}", entity.CorrelationId,
                rsmHolidayRequest, holidayRequest.HolidayRequestRef, "Dtos.Ufo.HolidayRequest", null, null, "Ufo.HolidayRequest");



            entity.ExportSuccess = true;
        }
    }
}
