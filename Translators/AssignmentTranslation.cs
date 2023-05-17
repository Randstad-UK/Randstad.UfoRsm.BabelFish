using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish;
using Randstad.UfoRsm.BabelFish.Dtos;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using Randstad.UfoRsm.BabelFish.Translators;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class AssignmentTranslation : TranslatorBase, ITranslator
    {
        private readonly Dictionary<string, string> _rateCodes;
        private readonly List<DivisionCode> _divisionCodes;

        public AssignmentTranslation(IProducerService producer, string routingKeyBase, Dictionary<string, string> rateCodes, ILogger logger, string opCosToSend, bool allowBlockByDivision, List<DivisionCode> divisionCodes) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
        {

            _rateCodes = rateCodes;
            _divisionCodes = divisionCodes;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Assignment") return;

            Assignment assign = null;
            try
            {
                assign = JsonConvert.DeserializeObject<Assignment>(entity.Payload);

                _logger.Debug("Received Routing Key: " + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, assign.AssignmentRef, null, null);
                if (entity.ReceivedOnRoutingKeyNodes != null && entity.ReceivedOnRoutingKeyNodes.Length == 9)
                {
                    if (string.IsNullOrEmpty(assign.CheckIn))
                    {
                        _logger.Warn($"Assignment {assign.AssignmentRef} is not checked in " + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, assign.AssignmentRef, null, null);
                        entity.ExportSuccess = false;
                        return;
                    }

                    if (assign.CheckIn.ToLower() == "checked in" &&
                        entity.ReceivedOnRoutingKeyNodes[8] != "startchecked")
                    {
                        _logger.Warn(
                            $"Assignment {assign.AssignmentRef} is Checked in but there is no startchecked on Routing Key",
                            entity.CorrelationId, entity, assign.AssignmentRef, null, null);
                    }

                    if (assign.CheckIn.ToLower() == "checked in" &&
                        entity.ReceivedOnRoutingKeyNodes[8] == "startchecked")
                    {
                        _logger.Debug(
                            $"Received Routing has startchecked and assignment {assign.AssignmentRef} is checked in",
                            entity.CorrelationId, entity, assign.AssignmentRef, null, null);
                    }


                }
                else
                {
                    _logger.Warn($"Assignment {assign.AssignmentRef} has no startchecked flag on routing key " + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, assign.AssignmentRef, null, null);
                }

                if (BlockExport(Mappers.MapOpCoFromName(assign.OpCo.Name)))
                {
                    _logger.Warn($"Assignment OpCo not live in RSM {assign.AssignmentRef} {assign.OpCo.Name}", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (BlockExportByDivision(assign.Division.Name))
                {
                    _logger.Warn($"Assignment Division not live in RSM for assignment {assign.AssignmentRef} {assign.Division.Name}", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(assign.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {assign.AssignmentRef} Opco", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(assign.PreferredPeriod))
                {
                    _logger.Warn($"Assignment {assign.AssignmentRef} is historic should not export", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (assign.InvoiceAddress == null)
                {
                    _logger.Warn($"Assignment {assign.AssignmentRef} does not have an invoice address set", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (assign.Hle.Unit == null || string.IsNullOrEmpty(assign.Hle.Unit.FinanceCode))
                {
                    _logger.Warn($"HLE for Assignment {assign.AssignmentRef} has no owning team or finance code is not set", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (assign.InvoicePerson == null && assign.Division.Name != "Tuition Services" && assign.Division.Name != "Student Support")
                {
                    _logger.Warn($"Invoice Person for {assign.AssignmentRef} is missing", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                //if (assign.Candidate.LiveInPayroll == null || assign.Candidate.LiveInPayroll == false) 
                //{
                //    _logger.Warn($"Candidate {assign.Candidate.CandidateRef} on Assignment {assign.AssignmentRef} is not live in Payroll or has no live in payroll set (probably because the candidate was created before employee file generated)", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);
                //    entity.ExportSuccess = true;
                //    return;
                //}

            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising Assignment from UFO {entity.ObjectId} - {exp.Message}");
            }

            _logger.Success($"Received Assignment {assign.AssignmentRef}", entity.CorrelationId, assign, assign.AssignmentRef, "Dtos.Ufo.Assignment", null);

            if (string.IsNullOrEmpty(assign.CheckIn) || assign.CheckIn != "Checked In")
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Assignment {assign.AssignmentRef} is not checked in";
                entity.ValidationErrors.Add(message);

                _logger.Warn(message, entity.CorrelationId, assign, assign.AssignmentRef, "Dtos.Ufo.Assignment", null);
                entity.ExportSuccess = false;
                return;

            }

            Dtos.RsmInherited.Placement assignment = null;

            try
            {
                assignment = assign.MapAssignment(_logger, _rateCodes, entity.CorrelationId, _divisionCodes);

            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map assignment {assign.AssignmentRef}: {exp.Message}", entity.CorrelationId, assignment, assign.AssignmentRef, "Assignment", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;

                return;
            }

            var eventType = "Update";
            if (entity.EventType == "CheckIn")
            {
                eventType = "Create";
            }

            if (assign.Division.Name == "Tuition Services" || assign.Division.Name == "Student Support")
            {
                SendToRsm(JsonConvert.SerializeObject(assignment), "sws", "Assignment", entity.CorrelationId, Helpers.Mappers.MapCheckin(assign.CheckIn), false, eventType);
                _logger.Success($"Successfully mapped Assignment {assign.AssignmentRef} and sent to SWS RSM", entity.CorrelationId, assignment, assign.AssignmentRef, "Dtos.Ufo.Assignment", null, null, "Dtos.Sti.Assignment");
            }
            else
            {
                SendToRsm(JsonConvert.SerializeObject(assignment), Mappers.MapOpCoFromName(assign.OpCo.Name).ToString(), "Assignment", entity.CorrelationId, Helpers.Mappers.MapCheckin(assign.CheckIn), false, eventType);
                _logger.Success($"Successfully mapped Assignment {assign.AssignmentRef} and sent to RSM", entity.CorrelationId, assignment, assign.AssignmentRef, "Dtos.Ufo.Assignment", null, null, "Dtos.Sti.Assignment");
            }

            entity.ExportSuccess = true;
        }
    }
}
