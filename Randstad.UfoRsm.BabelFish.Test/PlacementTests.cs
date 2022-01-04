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
    public class PlacementTests
    {
        private IProducerService _producer;
        private string _routingBase;
        private ExportedEntity _entity;
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _entity = new ExportedEntity();
            _entity.ObjectType = "Placement";
            _entity.EventType = "Update";
            _entity.CorrelationId = Guid.NewGuid();

            _routingBase = "staging.v2.rand.{opco}.ufobablefish.{object}.*.suc.*.startchecked";
            _producer = new Producer();
            _logger = new FakeLogger();
        }

        //[Test]
        //public async Task Should_Map_Placement()
        //{
        //    var tomCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.TomCodes);
        //    var empRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Test.Properties.Resources.EmployerRefs);
        //    _entity.Payload = Test.Properties.Resources.Placement;
        //    var sut = new PlacementTranslation(_producer, _routingBase, "Prefix", tomCodes, empRefs, _logger);
        //    await sut.Translate(_entity);
        //}

        //[Test]
        //public async Task Should_Map_Placement_Invoice_Address_From_Export_Entity()
        //{
        //    var tomCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.TomCodes);
        //    var employerRefs = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.EmployerRefs);

        //    var cand = Properties.Resources.Debug_Placement;
        //    var ent = new ExportedEntity();
        //    ent.CorrelationId = Guid.NewGuid();
        //    ent.EventType = "Update";
        //    ent.ExportDate = DateTime.Now;
        //    ent.ObjectId = "1234";
        //    ent.Payload = cand;
        //    ent.ObjectType = "Placement";

        //    var sut = new PlacementTranslation(_producer, _routingBase, "UFO", tomCodes, employerRefs, _logger);
        //    await sut.Translate(ent);
        //}
    }
}
