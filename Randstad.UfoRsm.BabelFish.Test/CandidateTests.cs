using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Tests.Fakes;
using Randstad.UfoRsm.BabelFish.Translators;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Tests
{
    public class CandidateTests
    {
        private IProducerService _producer;
        private string _routingBase;
        private Dictionary<string, string> _rates;
        private ExportedEntity _entity;
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _rates = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.RateMap);
            _entity = new ExportedEntity();
            _entity.ObjectType = "Candidate";
            _entity.EventType = "Update";
            _entity.CorrelationId = Guid.NewGuid();

            _routingBase = "test.v3.rand.{opco}.ufobablefish.{object}.ignore.suc{rule}";
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
        public async Task Should_Map_Paye_Candidate()
        {
            var tomCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.TomCodes);
            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.EmployerRefs);

            var cand = Test.Properties.Resources.Debug_PayeCandidate;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = cand;
            ent.ObjectType = "Candidate";

            _entity.Payload = cand;

            var sut = new CandidateTranslation(_producer, _routingBase, employerRefs, tomCodes, _logger);
            await sut.Translate(_entity);
        }

        [Test]
        public async Task Should_Map_Umbrella_Candidate()
        {
            var tomCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.TomCodes);
            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.EmployerRefs);

            var cand = Test.Properties.Resources.Debug_UmbrellaCandidate;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = cand;
            ent.ObjectType = "Candidate";

            _entity.Payload = cand;

            var sut = new CandidateTranslation(_producer, _routingBase, employerRefs, tomCodes, _logger);
            await sut.Translate(_entity);
        }

        [Test]
        public async Task Should_Map_Ltd_Candidate()
        {
            var tomCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.TomCodes);
            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.EmployerRefs);

            var cand = Test.Properties.Resources.Debug_LtdCandidate;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = cand;
            ent.ObjectType = "Candidate";

            _entity.Payload = cand;

            var sut = new CandidateTranslation(_producer, _routingBase, employerRefs, tomCodes, _logger);
            await sut.Translate(_entity);
        }



    }
}
