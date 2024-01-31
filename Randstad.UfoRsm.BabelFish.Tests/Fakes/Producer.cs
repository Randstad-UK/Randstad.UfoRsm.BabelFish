using System;
using System.Collections.Generic;
using System.Text;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Tests.Fakes
{
    public class Producer : IProducerService
    {
        public void Dispose()
        {

        }

        public void CloseConnection()
        {

        }

        public void Publish(Dictionary<string, object> Headers, Guid CorrelationId, string RoutingKey, string Body)
        {

        }
    }
}
