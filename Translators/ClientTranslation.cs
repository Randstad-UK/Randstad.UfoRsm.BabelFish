using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class ClientTranslation : TranslatorBase, ITranslator
    {

        public ClientTranslation(IProducerService producer, string routingKeyBase, ILogger logger) : base(producer, routingKeyBase, logger)
        {

        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Client") return;

            Dtos.Ufo.Client client;
            try
            {
                client = JsonConvert.DeserializeObject<Dtos.Ufo.Client>(entity.Payload);

                if (string.IsNullOrEmpty(client.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {client.ClientRef} Opco", entity.CorrelationId, entity, client.ClientRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(client.Unit.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {client.ClientRef} Unit", entity.CorrelationId, entity, client.ClientRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Client from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }



            _logger.Success($"Recieved Client {client.ClientRef}", entity.CorrelationId, client, client.ClientRef, "Dtos.Ufo.Client", null);

            if (client.IsCheckedIn == null || client.IsCheckedIn == false)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Client {client.ClientRef} is not checked in";
                _logger.Debug(message, entity.CorrelationId, message, client.ClientRef, "Dtos.Ufo.Client", null);
                entity.ValidationErrors.Add(message);
                return;
            }

            RSM.Client rmsClient = null;
            try
            {
                rmsClient = client.MapClient();
               
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map client {client.ClientRef} {exp.Message}", entity.CorrelationId, client, client.ClientRef, "Dtos.Ufo.Client", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }



            SendToRsm(JsonConvert.SerializeObject(rmsClient), Mappers.MapOpCoFromName(client.OpCo.Name.ToLower()).ToString(), "Client", entity.CorrelationId, (bool)client.IsCheckedIn);
            _logger.Success($"Successfully mapped Client {client.ClientRef} and Sent To RSM", entity.CorrelationId, client, client.ClientRef, "Dtos.Ufo.Client", null, rmsClient, "RSM.Client");
            entity.ExportSuccess = true;

        }


    }
}
