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
    class AssignmentTests
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
        public async Task Should_Map_Assignment_Paye()
        {
            var divisionCodes = JsonConvert.DeserializeObject <List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);


            var assignment = Properties.Resources.Debug_Assignment_Paye;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = assignment;
            ent.ObjectType = "Assignment";

            ent.Payload = assignment;

            var sut = new AssignmentTranslation(_producer, _routingBase, _rates, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Should_Map_Assignment_Ltd()
        {
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);


            var assignment = Properties.Resources.Debug_Assignment_Ltd;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = assignment;
            ent.ObjectType = "Assignment";

            ent.Payload = assignment;

            var sut = new AssignmentTranslation(_producer, _routingBase, _rates, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Debug_Testers()
        {

            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);

            var assignment = Properties.Resources.Debug_Assignment_Live;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = assignment;
            ent.ObjectType = "Assignment";

            ent.Payload = assignment;

            var sut = new AssignmentTranslation(_producer, _routingBase, _rates, _logger, "CPE,BS,CARE,RIS", false, divisionCodes);
            await sut.Translate(ent);
        }
    }
}
