using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish.Dtos;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class ClientTranslation : TranslatorBase, ITranslator
    {
        private List<DivisionCode> _divisionCodes;

        public ClientTranslation(IProducerService producer, string routingKeyBase, ILogger logger, string opCosToSend, bool allowBlockByDivision, List<DivisionCode> divisionCodes) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
        {
            _divisionCodes = divisionCodes;
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

                _logger.Debug("Received Routing Key: " + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, client.ClientRef, null, null);
                if (entity.ReceivedOnRoutingKeyNodes!=null && entity.ReceivedOnRoutingKeyNodes.Length == 9)
                {
                    if (client.IsCheckedIn == null)
                    {
                        _logger.Warn($"Client {client.ClientRef} is not checked in" + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, client.ClientRef, null, null);
                    }
                    else
                    {
                        if ((bool) client.IsCheckedIn && entity.ReceivedOnRoutingKeyNodes[8] != "startchecked")
                        {
                            _logger.Warn($"Client {client.ClientRef} is check in but there is no startchecked on the routing key" + entity.ReceivedOnRoutingKey, entity.CorrelationId,entity, client.ClientRef, null, null);
                        }

                        if ((bool)client.IsCheckedIn && entity.ReceivedOnRoutingKeyNodes[8] == "startchecked")
                        {
                            _logger.Debug($"Received Routing has startchecked and client {client.ClientRef} is live in payroll", entity.CorrelationId, entity, client.ClientRef, null, null);
                        }
                    }
                }
                else
                {
                    _logger.Warn($"Client {client.ClientRef} has no startchecked flag on routing key " + entity.ReceivedOnRoutingKey, entity.CorrelationId, entity, client.ClientRef, null, null);
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
                rsmClient = client.MapClient(_divisionCodes);
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem mapping Client from UFO {entity.ObjectId} - {exp.Message}");
            }

            if (client.Division.Name == "Tuition Services" || client.Division.Name == "Student Support")
            {
                SendToRsm(JsonConvert.SerializeObject(rsmClient), "sws", "Client", entity.CorrelationId, (bool)client.IsCheckedIn);
                _logger.Success($"Successfully mapped Client {client.ClientRef} and Sent To SWS RSM", entity.CorrelationId, rsmClient, client.ClientRef, "Dtos.Ufo.Client", null, null, "RSM.Client");
            }
            else
            {
                SendToRsm(JsonConvert.SerializeObject(rsmClient), Mappers.MapOpCoFromName(client.OpCo.Name.ToLower()).ToString(), "Client", entity.CorrelationId, (bool)client.IsCheckedIn);
                _logger.Success($"Successfully mapped Client {client.ClientRef} and Sent To RSM", entity.CorrelationId, rsmClient, client.ClientRef, "Dtos.Ufo.Client", null, null, "RSM.Client");
            }


            entity.ExportSuccess = true;

        }


    }
}
