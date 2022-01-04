using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoSti.BabelFish.Dtos.Ufo;
using RandstadMessageExchange;

namespace Randstad.UfoSti.BabelFish.Translators
{
    public class ClientContactTranslation : TranslatorBase, ITranslator
    {
        private readonly string _consultantCodePrefix;

        public ClientContactTranslation(IProducerService producer, string routingKeyBase, string consultantCodePrefix, ILogger logger) : base(producer, routingKeyBase, logger)
        {
            _consultantCodePrefix = consultantCodePrefix;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "ClientContact") return;

            var contact = JsonConvert.DeserializeObject<Dtos.Ufo.ClientContact>(entity.Payload);

            if ( contact.IsCheckedIn == false)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = "Client Contact is not checked in";
                _logger.Warn(message, entity.CorrelationId, contact, entity.ObjectId, "Dtos.Ufo.ClientContact", null);
                entity.ValidationErrors.Add(message);
                return;
            }

            entity.ValidationErrors = new List<string>();
            entity.ValidationErrors.Add("Client Contacts not mapped to STI");
            entity.ExportSuccess = false;

        }
    }
}
