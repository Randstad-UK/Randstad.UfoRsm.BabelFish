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
    class PlacementTests
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

            _routingBase = "dev.v1.rand.{opco}.uforsmbabelfish.{object}.ignore.suc{rule}";
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
        public async Task Should_Map_Placement()
        {
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);


            var assignment = Properties.Resources.Debug_Placement;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = assignment;
            ent.ObjectType = "Placement";

            ent.Payload = assignment;

            var sut = new PlacementTranslation(_producer, _routingBase, divisionCodes, _logger, "BS", true);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Should_Map_Placement_Testing()
        {
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);


            var assignment = Properties.Resources.Debug_Placement_Live;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = assignment;
            ent.ObjectType = "Placement";

            ent.Payload = assignment;

            var sut = new PlacementTranslation(_producer, _routingBase, divisionCodes, _logger, "CARE,BS,CPE", true);
            await sut.Translate(ent);
        }
    }
}
