using System;
using System.Collections.Generic;
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
    public class PlacementTranslation : TranslatorBase, ITranslator
    {
        private readonly string _consultantCodePrefix;
        private readonly List<DivisionCode> _divisionCodes;

        public PlacementTranslation(IProducerService producer, string routingKeyBase, List<DivisionCode> divisionCodes, ILogger logger, string opCosToSend, bool allowBlockByDivision) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
        {

            _divisionCodes = divisionCodes;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Placement") return;

            Placement placement = null;
            try
            {
                placement = JsonConvert.DeserializeObject<Placement>(entity.Payload);

                if (BlockExport(Mappers.MapOpCoFromName(placement.OpCo.Name)))
                {
                    _logger.Warn($"Placement OpCo not live in RSM {placement.PlacementRef} {placement.OpCo.Name}", entity.CorrelationId, entity, placement.PlacementRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (BlockExportByDivision(placement.Division.Name))
                {
                    _logger.Warn($"Candidate Division not live in RSM {placement.PlacementRef} {placement.Division.Name}", entity.CorrelationId, entity, placement.PlacementRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising Placement from UFO {entity.ObjectId} - {exp.Message}");
            }

            _logger.Success($"Received Placement {placement.PlacementRef}", entity.CorrelationId, placement, placement.PlacementRef, "Dtos.Ufo.Placement", null);

            if (string.IsNullOrEmpty(placement.CheckIn) || placement.CheckIn=="No Show")
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Placement {placement.PlacementRef} is not checked in";
                _logger.Warn(message, entity.CorrelationId, message, placement.PlacementRef, "Dtos.Ufo.Placement", null);
                entity.ValidationErrors.Add(message);

                entity.ExportSuccess = false;
                return;

            }

            RSM.Placement mappedPlacement = null;
            try
            {
                mappedPlacement = placement.MapPlacement(_logger, entity.CorrelationId, _divisionCodes);
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem mapping placement from UFO {entity.ObjectId} - {exp.Message}");
            }

            SendToRsm(JsonConvert.SerializeObject(mappedPlacement), Mappers.MapOpCoFromName(placement.OpCo.Name).ToString(), "Placement", entity.CorrelationId, Mappers.MapCheckin(placement.CheckIn));
            _logger.Success($"Successfully mapped Placement {placement.PlacementRef} and sent to RSM", entity.CorrelationId, mappedPlacement, placement.PlacementRef, "Dtos.Ufo.Placement", null, null, "Dtos.Sti.Placement");
            entity.ExportSuccess = true;
        }

    }
}
