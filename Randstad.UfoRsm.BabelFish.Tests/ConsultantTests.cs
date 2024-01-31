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
    class ConsultantTests
    {
        private IProducerService _producer;
        private string _routingBase;
        private ExportedEntity _entity;
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _entity = new ExportedEntity();
            _entity.ObjectType = "Consultant";
            _entity.EventType = "Update";
            _entity.CorrelationId = Guid.NewGuid();

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
        public async Task Should_Map_Consultant_Debug()
        {
            var export = new ExportedEntity();

            export.Payload = Properties.Resources.Debug_Consultant;
            export.EventType = "Update";
            export.IsCheckedIn = true;
            export.ObjectId = "1234";
            export.ObjectType = "Consultant";
            export.CorrelationId = Guid.NewGuid();

            var tomCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.TomCodes);
            var empRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            _entity.Payload = Properties.Resources.Debug_Consultant;
            var sut = new ConsultantTranslation(_producer, _routingBase, _logger, "BS", true);
            await sut.Translate(export);
        }

        [Test]
        public async Task Should_Map_Consultant_Debug_Live()
        {
            var export = new ExportedEntity();

            export.Payload = Properties.Resources.Debug_Consultant_Live;
            export.EventType = "Update";
            export.IsCheckedIn = true;
            export.ObjectId = "1234";
            export.ObjectType = "Consultant";
            export.CorrelationId = Guid.NewGuid();

            var tomCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.TomCodes);
            var empRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            _entity.Payload = Properties.Resources.Debug_Consultant;
            var sut = new ConsultantTranslation(_producer, _routingBase, _logger, "BS", true);
            await sut.Translate(export);
        }
    }
}
