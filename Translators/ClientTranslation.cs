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

        public ClientTranslation(IProducerService producer, string routingKeyBase, ILogger logger, string opCosToSend, bool allowBlockByDivision) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
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
                _logger.Debug($"Logging failed client export entity", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                throw new Exception($"Problem deserialising Client from UFO {entity.ObjectId} - {exp.Message}");
            }



            _logger.Success($"Received Client {client.ClientRef}", entity.CorrelationId, client, client.ClientRef, "Dtos.Ufo.Client", null);

            if (client.IsCheckedIn == null || client.IsCheckedIn == false)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Client {client.ClientRef} is not checked in";
                _logger.Warn(message, entity.CorrelationId, entity, client.ClientRef, "Dtos.Ufo.Client", null);
                entity.ValidationErrors.Add(message);
                return;
            }

            RSM.Client rsmClient = null;
            try
            {
                rsmClient = client.MapClient();
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem mapping Client from UFO {entity.ObjectId} - {exp.Message}");
            }

            //RSM.Client hleClient = null;
            //if (client.HleClient != null)
            //{
            //    hleClient = client.HleClient.MapClient();
            //}

            ////if HLE has been mapped then send to RSM
            //if (hleClient != null)
            //{
            //    SendToRsm(JsonConvert.SerializeObject(hleClient), Mappers.MapOpCoFromName(client.OpCo.Name.ToLower()).ToString(), "Client", entity.CorrelationId, (bool)client.IsCheckedIn);
            //    _logger.Success($"Successfully mapped HLE Client {client.ClientRef} and Sent To RSM", entity.CorrelationId, rsmClient, client.ClientRef, "Dtos.Ufo.Client", null, null, "RSM.Client");
            //}

            SendToRsm(JsonConvert.SerializeObject(rsmClient), Mappers.MapOpCoFromName(client.OpCo.Name.ToLower()).ToString(), "Client", entity.CorrelationId, (bool)client.IsCheckedIn);
            _logger.Success($"Successfully mapped Client {client.ClientRef} and Sent To RSM", entity.CorrelationId, rsmClient, client.ClientRef, "Dtos.Ufo.Client", null, null, "RSM.Client");
            entity.ExportSuccess = true;

        }


    }
}
