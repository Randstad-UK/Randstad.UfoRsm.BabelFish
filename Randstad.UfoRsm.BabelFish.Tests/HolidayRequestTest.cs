using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Tests.Fakes;
using Randstad.UfoRsm.BabelFish.Translators;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Tests
{
    class HolidayRequestTest
    {
        private IProducerService _producer;
        private string _routingBase;
        private Dictionary<string, string> _rates;
        private ExportedEntity _entity;
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _rates = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.RateMap);
            _entity = new ExportedEntity();
            _entity.ObjectType = "HolidayRequest";
            _entity.EventType = "Update";
            _entity.CorrelationId = Guid.NewGuid();

            _routingBase = "test.v1.rand.{opco}.uforsmbabelfish.{object}.ignore.suc{rule}";
            var producerSettings = new Dictionary<string, string>
            {
                { "Host", "euukdoprmq001.ukdta.co.uk" },
                { "Username", "RabbitDTA" },
                { "Password", "RabbitDTA" },
                { "ExchangeName", "RandstadExchangeDTA" },
                { "Port", "5672" }
            };

            _producer = new ProducerService(producerSettings);

            _logger = new FakeLogger();
        }

        [Test]
        public async Task Should_Map_HolidayRequest()
        {

            var holiday = Properties.Resources.Debug_HolidayRequest;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = holiday;
            ent.ObjectType = "Absence";

            _entity.Payload = holiday;
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new HolidayRequestTranslation(_producer, _routingBase, _logger, "BS", true, divisionCodes);
            await sut.Translate(_entity);
        }

        [Test]
        public async Task Should_Map_HolidayRequest_Tester()
        {

            var holiday = Properties.Resources.Debug_Holiday_Request_live;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = holiday;
            ent.ObjectType = "HolidayRequest";

            _entity.Payload = holiday;
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new HolidayRequestTranslation(_producer, _routingBase, _logger, "RIS", true, divisionCodes);
            await sut.Translate(_entity);
        }
    }
}
