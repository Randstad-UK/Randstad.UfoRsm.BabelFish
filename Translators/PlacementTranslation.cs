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

namespace Randstad.UfoSti.BabelFish.Translators
{
    public class PlacementTranslation : TranslatorBase, ITranslator
    {
        private readonly string _consultantCodePrefix;
        private readonly Dictionary<string, string> _tomCodes;
        private readonly Dictionary<string, string> _employerRefs;

        public PlacementTranslation(IProducerService producer, string routingKeyBase, string consultantCodePrefix, Dictionary<string, string> tomCodes, Dictionary<string, string> employerRefs, ILogger logger) : base(producer, routingKeyBase, logger)
        {
            _consultantCodePrefix = consultantCodePrefix;
            _tomCodes = tomCodes;
            _employerRefs = employerRefs;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Placement") return;

            Dtos.Ufo.Placement placement = null;
            try
            {
                placement = JsonConvert.DeserializeObject<Dtos.Ufo.Placement>(entity.Payload);


            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Placement from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Placement {placement.PlacementRef}", entity.CorrelationId, placement, entity.ObjectId, "Dtos.Ufo.Placement", null);

            if (string.IsNullOrEmpty(placement.CheckIn) || placement.CheckIn=="No Show")
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Placement {placement.PlacementRef} is not checked in";
                _logger.Warn(message, entity.CorrelationId, message, entity.ObjectId, "Dtos.Ufo.Placement", null);
                entity.ValidationErrors.Add(message);

                entity.ExportSuccess = false;
                return;

            }

            Dtos.Sti.Placement mappedPlacement = null;
            ClientAddress invoiceAddress = null;
            try
            {
                mappedPlacement = placement.MapPlacement(_consultantCodePrefix, _tomCodes, _employerRefs, out invoiceAddress);
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map placement {placement.PlacementRef}: {exp.Message}", entity.CorrelationId, placement, entity.ObjectId, "Dtos.Ufo.Placement", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }

            if (invoiceAddress != null)
            {
                SendClientAddressToOpcos(invoiceAddress, entity.CorrelationId, placement.InvoiceAddress, entity.ObjectId, placement.PlacementRef);
            }

            SendToSti(JsonConvert.SerializeObject(mappedPlacement), mappedPlacement.OpCo.ToString(), "Placement", entity.CorrelationId, (bool)mappedPlacement.IsStartChecked);
            _logger.Success($"Successfully mapped Placement {placement.PlacementRef} and sent to Sti", entity.CorrelationId, placement, entity.ObjectId, "Dtos.Ufo.Placement", null, mappedPlacement, "Dtos.Sti.Placement");
            entity.ExportSuccess = true;
        }

        private void SendClientAddressToOpcos(ClientAddress clientAddresses, Guid correlationId,
            Dtos.Ufo.InvoiceAddress ufoInvoiceAddress, string entityId, string placementRef)
        {

            foreach (var opco in _employerRefs)
            {
                var o = opco.Key;
                clientAddresses.EmployerRef = opco.Value;

                if (o.ToLower() == "ps")
                {
                    o = "care";
                }

                //Is Checked in always true as UFO won't export unless it is
                SendToSti(JsonConvert.SerializeObject(clientAddresses), o, "ClientAddress", correlationId, true);
                _logger.Success(
                    $"Successfully mapped ClientAddress {ufoInvoiceAddress.InvoiceAddressRef} for placement {placementRef} and Sent To {o} STI",
                    correlationId, ufoInvoiceAddress, ufoInvoiceAddress.AddressId, "Dtos.Ufo.InvoiceAddress", null,
                    clientAddresses, "Dtos.Sti.ClientAddress");
            }
        }
    }
}
