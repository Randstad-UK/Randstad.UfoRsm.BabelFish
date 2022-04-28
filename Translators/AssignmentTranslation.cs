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
    public class AssignmentTranslation : TranslatorBase, ITranslator
    {
        private readonly Dictionary<string, string> _rateCodes;
        private readonly Dictionary<string, string> _tomCodes;

        public AssignmentTranslation(IProducerService producer, string routingKeyBase, Dictionary<string, string> tomCodes, Dictionary<string, string> rateCodes, ILogger logger, string opCosToSend) : base(producer, routingKeyBase, logger, opCosToSend)
        {
            _tomCodes = tomCodes;
            _rateCodes = rateCodes;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Assignment") return;

            Assignment assign = null;
            try
            {
                assign = JsonConvert.DeserializeObject<Assignment>(entity.Payload);

                if (BlockExport(Mappers.MapOpCoFromName(assign.OpCo.Name)))
                {
                    _logger.Warn($"Assignment OpCo not live in RSM {assign.AssignmentRef} {assign.OpCo.Name}", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
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

                if (assign.Hle.Unit == null || string.IsNullOrEmpty(assign.Hle.Unit.FinanceCode))
                {
                    _logger.Warn($"HLE for Assignment {assign.AssignmentRef} has no owning team or finance code is not set", entity.CorrelationId, entity, assign.AssignmentRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Assignment from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Assignment {assign.AssignmentRef}", entity.CorrelationId, assign, assign.AssignmentRef, "Dtos.Ufo.Assignment", null);

            if (string.IsNullOrEmpty(assign.CheckIn) || assign.CheckIn!="Checked In")
            {
                if(entity.ValidationErrors==null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Assignment {assign.AssignmentRef} is not checked in";
                entity.ValidationErrors.Add(message);

                _logger.Debug(message, entity.CorrelationId, assign, assign.AssignmentRef, "Dtos.Ufo.Assignment", null);
                entity.ExportSuccess = false;
                return;
                
            }

            Dtos.RsmInherited.Placement assignment = null;
            
            try
            {
                assignment = assign.MapAssignment(_tomCodes, _logger, _rateCodes, entity.CorrelationId);

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


            SendToRsm(JsonConvert.SerializeObject(assignment), Mappers.MapOpCoFromName(assign.OpCo.Name).ToString(), "Assignment", entity.CorrelationId, Helpers.Mappers.MapCheckin(assign.CheckIn));

            _logger.Success($"Successfully mapped Assignment {assign.AssignmentRef} and sent to RSM", entity.CorrelationId, assignment, assign.AssignmentRef, "Dtos.Ufo.Assignment", null, null, "Dtos.Sti.Assignment");
            
            entity.ExportSuccess = true;
        }
    }
}
