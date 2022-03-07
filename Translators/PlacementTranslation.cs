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
        private readonly Dictionary<string, string> _tomCodes;

        public PlacementTranslation(IProducerService producer, string routingKeyBase, Dictionary<string, string> tomCodes, ILogger logger, bool systemUnderTest) : base(producer, routingKeyBase, logger, systemUnderTest)
        {

            _tomCodes = tomCodes;
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
                    _logger.Warn($"Placement OpCo not live in RSWM {placement.PlacementRef} {placement.OpCo.Name}", entity.CorrelationId, entity, placement.PlacementRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }
            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Placement from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Placement {placement.PlacementRef}", entity.CorrelationId, placement, placement.PlacementRef, "Dtos.Ufo.Placement", null);

            if (string.IsNullOrEmpty(placement.CheckIn) || placement.CheckIn=="No Show")
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Placement {placement.PlacementRef} is not checked in";
                _logger.Debug(message, entity.CorrelationId, message, placement.PlacementRef, "Dtos.Ufo.Placement", null);
                entity.ValidationErrors.Add(message);

                entity.ExportSuccess = false;
                return;

            }

            RSM.Placement mappedPlacement = null;
            try
            {
                mappedPlacement = placement.MapPlacement(_tomCodes, _logger, entity.CorrelationId);
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map placement {placement.PlacementRef}: {exp.Message}", entity.CorrelationId, placement, placement.PlacementRef, "Dtos.Ufo.Placement", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }

            SendToRsm(JsonConvert.SerializeObject(mappedPlacement), Mappers.MapOpCoFromName(placement.OpCo.Name).ToString(), "Placement", entity.CorrelationId, Mappers.MapCheckin(placement.CheckIn));
            _logger.Success($"Successfully mapped Placement {placement.PlacementRef} and sent to RSM", entity.CorrelationId, mappedPlacement, placement.PlacementRef, "Dtos.Ufo.Placement", null, null, "Dtos.Sti.Placement");
            entity.ExportSuccess = true;
        }

    }
}
