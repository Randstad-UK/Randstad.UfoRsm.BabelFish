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
    public class ClientTests
    {
        private IProducerService _producer;
        private string _routingBase;
        private ExportedEntity _entity;
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _entity = new ExportedEntity();
            _entity.ObjectType = "Client";
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


        //[Test]
        //public async Task Should_Map_Client_From_Export()
        //{
        //    var client = Properties.Resources.Debug_Client;

        //    var ent = new ExportedEntity();
        //    ent.CorrelationId = Guid.NewGuid();
        //    ent.EventType = "Update";
        //    ent.ExportDate = DateTime.Now;
        //    ent.ObjectId = "1234";
        //    ent.Payload = client;
        //    ent.ObjectType = "Client";

        //    var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
        //    var tomCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.TomCodes);
        //    var sut = new ClientTranslation(_producer, _routingBase, _logger, "BS", true);
        //    await sut.Translate(ent);
        //}

        [Test]
        public async Task Debug_Tests()
        {
            var client = Properties.Resources.Debug_Client_Live;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = client;
            ent.ObjectType = "Client";

            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new ClientTranslation(_producer, _routingBase, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Child_Billing_Controlled()
        {
            var client = Properties.Resources.ChildBillingControlled;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = client;
            ent.ObjectType = "Client";

            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new ClientTranslation(_producer, _routingBase, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Child_Electronic()
        {
            var client = Properties.Resources.ChildBillingControlled;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = client;
            ent.ObjectType = "Client";

            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new ClientTranslation(_producer, _routingBase, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Child_SelfBill()
        {
            var client = Properties.Resources.ChildSelfBill;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = client;
            ent.ObjectType = "Client";

            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new ClientTranslation(_producer, _routingBase, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Hle_BillingControlled()
        {
            var client = Properties.Resources.HleBillingControlled;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = client;
            ent.ObjectType = "Client";

            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new ClientTranslation(_producer, _routingBase, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Hle_SelfBill()
        {
            var client = Properties.Resources.HleSelfBill;

            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = client;
            ent.ObjectType = "Client";

            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new ClientTranslation(_producer, _routingBase, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }

        [Test]
        public async Task Hle_Electronic()
        {
            var client = Properties.Resources.HleElectronic;
            var ent = new ExportedEntity();
            ent.CorrelationId = Guid.NewGuid();
            ent.EventType = "Update";
            ent.ExportDate = DateTime.Now;
            ent.ObjectId = "1234";
            ent.Payload = client;
            ent.ObjectType = "Client";

            var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);
            var divisionCodes = JsonConvert.DeserializeObject<List<DivisionCode>>(Properties.Resources.DivisionCodes);
            var sut = new ClientTranslation(_producer, _routingBase, _logger, "BS", true, divisionCodes);
            await sut.Translate(ent);
        }
    }
}
