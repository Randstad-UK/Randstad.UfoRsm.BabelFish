using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Tests.Fakes;
using Randstad.UfoRsm.BabelFish.Translators;
using RandstadMessageExchange;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Tests
{
    class TimesheetTests
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


        //[Test]
        //public async Task Should_Map_Timesheet()
        //{
        //    var export = new ExportedEntity();

        //    export.Payload = Properties.Resources.Debug_Timesheet;
        //    export.EventType = "Update";
        //    export.IsCheckedIn = true;
        //    export.ObjectId = "1234";
        //    export.ObjectType = "Timesheet";
        //    export.CorrelationId = Guid.NewGuid();

        //    var rateCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.RateMap);
        //    var sut = new TimesheetTranslation(_producer, _routingBase, _logger, rateCodes, "BS");
        //    await sut.Translate(export);
        //}

        [Test]
        public async Task Should_Map_Timesheet_Testing()
        {
            var export = new ExportedEntity();

            export.Payload = Properties.Resources.Debug_Timesheet_Live;
            export.EventType = "Update";
            export.IsCheckedIn = true;
            export.ObjectId = "1234";
            export.ObjectType = "Timesheet";
            export.CorrelationId = Guid.NewGuid();

            var rateCodes = JsonConvert.DeserializeObject<Dictionary<string, string>>(Properties.Resources.RateMap);
            var sut = new TimesheetTranslation(_producer, _routingBase, _logger, rateCodes, "Care,BS,CPE", false);
            await sut.Translate(export);
        }

    }
}
