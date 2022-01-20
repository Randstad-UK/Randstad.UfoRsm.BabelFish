using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class ClientContactTranslation : TranslatorBase, ITranslator
    {

        public ClientContactTranslation(IProducerService producer, string routingKeyBase, ILogger logger) : base(producer, routingKeyBase, logger)
        {

        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "ClientContact") return;

            ClientContact contact = null;
            RSM.Contact mappedContact = null;
            try
            {
                contact = JsonConvert.DeserializeObject<Dtos.Ufo.ClientContact>(entity.Payload);

                if (BlockExport(Mappers.MapOpCoFromName(contact.OpCo.Name)))
                {
                    _logger.Warn(
                        $"Contact OpCo not live in RSWM {contact.Forename} {contact.Surname} {contact.OpCo.Name}",
                        entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (contact.IsCheckedIn == false)
                {
                    if (entity.ValidationErrors == null)
                        entity.ValidationErrors = new List<string>();

                    var message = $"Client Contact {contact.Forename} {contact.Surname} is not checked in";
                    _logger.Warn(message, entity.CorrelationId, contact, entity.ObjectId, "Dtos.Ufo.ClientContact",
                        null);
                    entity.ValidationErrors.Add(message);
                    return;
                }

                _logger.Success($"Recieved ClientContact {contact.Forename} {contact.Surname}", entity.CorrelationId, contact, entity.ObjectId, "Dtos.Ufo.Candidate", null);

                mappedContact = contact.MapContact();
            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising ClientContact from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            SendToRsm(JsonConvert.SerializeObject(mappedContact), Mappers.MapOpCoFromName(contact.OpCo.Name.ToLower()).ToString(), "Client", entity.CorrelationId, (bool)contact.IsCheckedIn);
            _logger.Success($"Successfully mapped ClientContact {contact.Forename} {contact.Surname} and Sent To RSM", entity.CorrelationId, mappedContact, contact.ContactId, "Dtos.Ufo.Client", null, contact, "RSM.Client");
            entity.ExportSuccess = true;

        }
    }
}
