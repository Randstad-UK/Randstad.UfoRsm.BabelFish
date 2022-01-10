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

        public AssignmentTranslation(IProducerService producer, string routingKeyBase, Dictionary<string, string> tomCodes, Dictionary<string, string> rateCodes, ILogger logger) : base(producer, routingKeyBase, logger)
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
                    _logger.Warn($"Assignment OpCo not live in RSWM {assign.AssignmentRef} {assign.OpCo.Name}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(assign.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {assign.AssignmentRef} Opco", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(assign.PreferredPeriod))
                {
                    _logger.Warn($"Assignment {assign.AssignmentRef} is historic should not export", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
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

            _logger.Success($"Recieved Assignment {assign.AssignmentRef}", entity.CorrelationId, assign, entity.ObjectId, "Dtos.Ufo.Assignment", null);

            if (string.IsNullOrEmpty(assign.CheckIn) || assign.CheckIn!="Checked In")
            {
                if(entity.ValidationErrors==null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Assignment {assign.AssignmentRef} is not checked in";
                entity.ValidationErrors.Add(message);

                _logger.Warn(message, entity.CorrelationId, assign, entity.ObjectId, "Dtos.Ufo.Assignment", null);
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

                _logger.Warn($"Failed to map assignment {assign.AssignmentRef}: {exp.Message}", entity.CorrelationId, assignment, entity.ObjectId, "Assignment", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;

                return;
            }


            SendToRsm(JsonConvert.SerializeObject(assignment), Mappers.MapOpCoFromName(assign.OpCo.Name).ToString(), "Assignment", entity.CorrelationId, Helpers.Mappers.MapCheckin(assign.CheckIn));

            _logger.Success($"Successfully mapped Assignment {assign.AssignmentRef} and sent to RSM", entity.CorrelationId, assign, entity.ObjectId, "Dtos.Ufo.Assignment", null, assignment, "Dtos.Sti.Assignment");
            
            entity.ExportSuccess = true;
        }
    }
}
