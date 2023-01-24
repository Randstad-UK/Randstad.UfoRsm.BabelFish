using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos;
using Randstad.UfoRsm.BabelFish.Dtos.RsmInherited;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using Randstad.UfoRsm.BabelFish.Translators;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class HolidayRequestTranslation : TranslatorBase, ITranslator
    {
        private List<DivisionCode> _divisionCodes;

        public HolidayRequestTranslation(IProducerService producer, string routingKeyBase, ILogger logger, string opCosToSend, bool allowBlockByDivision, List<DivisionCode> divisionCodes) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
        {
            _divisionCodes = divisionCodes;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "HolidayRequest") return;

            Randstad.UfRsm.BabelFish.Dtos.Ufo.HolidayRequest holidayRequest = null;

            try
            {
                holidayRequest = JsonConvert.DeserializeObject<Randstad.UfRsm.BabelFish.Dtos.Ufo.HolidayRequest>(entity.Payload);

                _logger.Debug("Received Routing Key: " + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, holidayRequest.HolidayRequestRef, null, null);
                if (entity.ReceivedOnRoutingKeyNodes != null && entity.ReceivedOnRoutingKeyNodes.Length == 9)
                {
                    if ((bool)holidayRequest.Candidate.LiveInPayroll && entity.ReceivedOnRoutingKeyNodes[8] != "startchecked")
                    {
                        _logger.Warn($"Holiday request for candidate {holidayRequest.Candidate.CandidateRef} is checked in but there is no startchecked on the routing key" + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, holidayRequest.HolidayRequestRef, null, null);
                    }

                    if ((bool)holidayRequest.Candidate.LiveInPayroll && entity.ReceivedOnRoutingKeyNodes[8] == "startchecked")
                    {
                        _logger.Debug($"Received Routing has startchecked and candidate {holidayRequest.Candidate.CandidateRef} is live in payroll", entity.CorrelationId, entity, holidayRequest.HolidayRequestRef, null, null);
                    }
                }
                else
                {
                    _logger.Warn($"Candidate {holidayRequest.Candidate.CandidateRef} on Holiday Request {holidayRequest.HolidayRequestRef} has no startchecked flag on routing key " + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, holidayRequest.HolidayRequestRef, null, null);
                }

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


            RSM.HolidayClaim rsmHolidayRequest = null;
            try
            {
                rsmHolidayRequest = holidayRequest.MapHolidayRequest(_divisionCodes, _logger, entity.CorrelationId);
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

            if (holidayRequest.Candidate.Division.Name == "Tuition Services" || holidayRequest.Candidate.Division.Name == "Student Support")
            {
                SendToRsm(JsonConvert.SerializeObject(rsmHolidayRequest), "sws", "holidayclaim", entity.CorrelationId, true);

                _logger.Success($"Successfully sent holidayrequest {holidayRequest.Candidate.CandidateRef} for {holidayRequest.Candidate.OperatingCo.Name} on {_updatedRoutingKey} to sws RSM", entity.CorrelationId,
                    rsmHolidayRequest, holidayRequest.HolidayRequestRef, "Dtos.Ufo.HolidayRequest", null, null, "Ufo.HolidayRequest");
            }
            else
            {
                SendToRsm(JsonConvert.SerializeObject(rsmHolidayRequest), Mappers.MapOpCoFromName(holidayRequest.Candidate.OperatingCo.Name).ToString(), "holidayclaim", entity.CorrelationId, true);

                _logger.Success($"Successfully sent holidayrequest {holidayRequest.Candidate.CandidateRef} to {holidayRequest.Candidate.OperatingCo.Name} on {_updatedRoutingKey} to RSM", entity.CorrelationId,
                    rsmHolidayRequest, holidayRequest.HolidayRequestRef, "Dtos.Ufo.HolidayRequest", null, null, "Ufo.HolidayRequest");
            }





            entity.ExportSuccess = true;
        }
    }
}
