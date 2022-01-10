using System;
using System.Collections.Generic;
using System.Text;
using Randstad.Logging;
using Randstad.OperatingCompanies;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public abstract class TranslatorBase
    {
        private readonly IProducerService _producer;
        private readonly string _routingKeyBase;
        protected readonly ILogger _logger;
        protected string _updatedRoutingKey;

        protected TranslatorBase(IProducerService producer, string routingKeyBase, ILogger logger)
        {
            _producer = producer;
            _routingKeyBase = routingKeyBase;
            _logger = logger;
        }

        protected void SendToRsm(string body, string opCo, string obj, Guid correlationId, bool isCheckedIn)
        {
            try
            {
                var routingKey = _routingKeyBase.Replace("{opco}", opCo.ToLower());
                routingKey = routingKey.Replace("{object}", obj);

                if (isCheckedIn)
                {
                    routingKey = routingKey.Replace("{rule}", ".startchecked");
                }
                else
                {
                    routingKey = routingKey.Replace("{rule}", string.Empty);
                }

                routingKey = routingKey.ToLower();

                var headers = new Dictionary<string, object>
                {
                    {"CorrelationId", correlationId.ToString("D")},
                    {"OpCo", opCo}

                };

                _updatedRoutingKey = routingKey;
                _producer.Publish(headers, correlationId, routingKey, body);
            }
            catch (Exception exp)
            {
                throw new Exception("Producer Failed To Send Message", exp);
            }
        }

        protected bool ShouldExportFromExport(string tomCode)
        {

            var validList = new List<string>()
            {
                "C1304",
                "C1326",
                "C1323",
                "C1324",
                "E1264",
                "E1261",
                "E1265",
                "E1262",
                "E1263",
                "B5504",
                "B5505",
                "B5501",
                "B5502",
                "B1384",
                "B1386",
                "P0034",
                "P0035"
            };

            var valid = validList.Contains(tomCode);



            return valid;

        }

        protected bool BlockExport(OperatingCompanies.OperatingCompany opco)
        {
            switch (opco)
            {
                case OperatingCompany.BS:
                    return false;
                default:
                    return true;
            }
        }
    }
}
