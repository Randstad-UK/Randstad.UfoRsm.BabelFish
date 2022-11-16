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

        public ClientContactTranslation(IProducerService producer, string routingKeyBase, ILogger logger, string opCosToSend, bool allowBlockByDivision) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
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

                _logger.Success($"Received ClientContact {contact.Forename} {contact.Surname}", entity.CorrelationId, contact, entity.ObjectId, "Dtos.Ufo.Candidate", null);

                mappedContact = contact.MapContact();
            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising ClientContact from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            if (contact.Division.Name == "Tuition Services" || contact.Division.Name == "Student Support")
            {
                SendToRsm(JsonConvert.SerializeObject(mappedContact), "sws", "Client", entity.CorrelationId, (bool)contact.IsCheckedIn);
                _logger.Success($"Successfully mapped ClientContact {contact.Forename} {contact.Surname} and Sent To SWS RSM", entity.CorrelationId, contact, contact.ContactId, "Dtos.Ufo.Client", null, null, "RSM.Client");
            }
            else
            {
                SendToRsm(JsonConvert.SerializeObject(mappedContact), Mappers.MapOpCoFromName(contact.OpCo.Name.ToLower()).ToString(), "Client", entity.CorrelationId, (bool)contact.IsCheckedIn);
                _logger.Success($"Successfully mapped ClientContact {contact.Forename} {contact.Surname} and Sent To RSM", entity.CorrelationId, contact, contact.ContactId, "Dtos.Ufo.Client", null, null, "RSM.Client");
            }
            
            entity.ExportSuccess = true;

        }
    }
}
