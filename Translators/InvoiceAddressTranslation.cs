using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.OperatingCompanies;
using Randstad.UfoSti.BabelFish.Dtos.Sti;
using Randstad.UfoSti.BabelFish.Dtos.Ufo;
using RandstadMessageExchange;

namespace Randstad.UfoSti.BabelFish.Translators
{
    public class InvoiceAddressTranslation : TranslatorBase, ITranslator
    {
        private readonly Dictionary<string, string> _employerRefs;

        public InvoiceAddressTranslation(IProducerService producer, string routingKeyBase, Dictionary<string, string> employerRefs, ILogger logger) : base(producer, routingKeyBase, logger)
        {
            _employerRefs = employerRefs;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "InvoiceAddress") return;

            Dtos.Ufo.InvoiceAddress invoiceAddress = null;
            try
            {
                invoiceAddress = JsonConvert.DeserializeObject<Dtos.Ufo.InvoiceAddress>(entity.Payload);

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Invoice Address from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Invoice Address {invoiceAddress.InvoiceAddressRef}", entity.CorrelationId, invoiceAddress, entity.ObjectId, "Dtos.Ufo.InvoiceAddress", null);

            List<ClientAddress> clientAddressList = null;
            try
            {
                clientAddressList = invoiceAddress.MapClientAddress();
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map invoice address {invoiceAddress.InvoiceAddressRef}", entity.CorrelationId, invoiceAddress, entity.ObjectId, "Dtos.Ufo.InvoiceAddress", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }


            SendToOpcos(clientAddressList, entity.CorrelationId, invoiceAddress);
            entity.ExportSuccess = true;

        }

        private void SendToOpcos(List<ClientAddress> clientAddresses, Guid correlationId, Dtos.Ufo.InvoiceAddress ufoInvoiceAddress)
        {
            foreach (ClientAddress addr in clientAddresses)
            {
                foreach (var opco in _employerRefs)
                {
                    var o = opco.Key;
                    addr.EmployerRef = opco.Value;

                    if (o.ToLower() == "ps")
                    {
                        o = "care";
                    }

                    //Is Checked in always true as UFO won't export unless it is
                    SendToSti(JsonConvert.SerializeObject(addr), o, "ClientAddress", correlationId, true);
                    _logger.Success($"Successfully mapped ClientAddress {ufoInvoiceAddress.InvoiceAddressRef} and Sent To {o} STI", correlationId, ufoInvoiceAddress, ufoInvoiceAddress.AddressId, "Dtos.Ufo.InvoiceAddress", null, addr, "Dtos.Sti.ClientAddress");
                }
            }

        }
    }
}
