using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
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
        private readonly string _opCosToSend;
        private readonly bool _allowBlockByDivision;

        protected TranslatorBase(IProducerService producer, string routingKeyBase, ILogger logger, string opCosToSend, bool allowBlockByDivision)
        {
            _producer = producer;
            _routingKeyBase = routingKeyBase;
            _logger = logger;
            _opCosToSend = opCosToSend;
            _allowBlockByDivision = allowBlockByDivision;
        }

        protected void SendToRsm(string body, string opCo, string obj, Guid correlationId, bool isCheckedIn, bool? processAdjustment = null)
        {
            try
            {
                var routingKey = _routingKeyBase.Replace("{opco}", opCo.ToLower());

                if (isCheckedIn && processAdjustment == null)
                {
                    routingKey = routingKey.Replace("{rule}", ".startchecked");
                }

                if (processAdjustment == true)
                {
                    routingKey = routingKey.Replace("{rule}", ".startchecked.adjustment");
                }
                else
                {
                    routingKey = routingKey.Replace("{rule}", string.Empty);
                }

                routingKey = routingKey.Replace("{object}", obj);

                routingKey = routingKey.ToLower();

                _logger.Debug("Routing Key: " + routingKey, correlationId, null, null, null, null);

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

        protected bool BlockExport(OperatingCompanies.OperatingCompany opco)
        {
            var opCos = _opCosToSend.Split(",").ToList();

            var block = true;
            foreach (var s in opCos)
            {
                if (s.Trim().ToUpper() == opco.ToString().ToUpper())
                {
                    block = false;
                    break;
                }
            }

            return block;
        }

        protected bool BlockExportByDivision(string divisionName)
        {
            if (!_allowBlockByDivision) return false;

            if (divisionName == "Tuition Services" || divisionName == "Student Support")
            {
                return true;
            }

            return false;
        }
    }
}
