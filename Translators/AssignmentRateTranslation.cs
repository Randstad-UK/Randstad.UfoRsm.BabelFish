﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish;
using Randstad.UfoRsm.BabelFish.Dtos.RsmInherited;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using Randstad.UfoRsm.BabelFish.Translators;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class AssignmentRateTranslation : TranslatorBase, ITranslator
    {
        private readonly Dictionary<string, string> _rateCodes;

        public AssignmentRateTranslation(Dictionary<string, string> rateCodes, IProducerService producer, string routingKeyBase, ILogger logger) : base(producer, routingKeyBase, logger)
        {
            _rateCodes = rateCodes;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "AssignmentRate") return;

            AssignmentRate rate = null;
            try
            {
                rate = JsonConvert.DeserializeObject<AssignmentRate>(entity.Payload);

                if (BlockExport(Mappers.MapOpCoFromName(rate.Assignment.OpCo.Name)))
                {
                    _logger.Warn($"Assignment OpCo not live in RSWM for assignment {rate.Assignment.AssignmentRef} {rate.Assignment.OpCo.Name}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(rate.Assignment.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {rate.FeeRef} Opco", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(rate.Assignment.PreferredPeriod))
                {
                    _logger.Warn($"Assignment Rate ${rate.FeeRef} attached assignment {rate.Assignment.AssignmentRef} is historic should not export", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Assignment Rate from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Assignment Rate for Assignment {rate.Assignment.AssignmentRef}", entity.CorrelationId, rate, entity.ObjectId, "Dtos.Ufo.AssignmentRate", null);

            if (string.IsNullOrEmpty(rate.Assignment.CheckIn) || rate.Assignment.CheckIn != "Checked In")
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Rate Assignment {rate.Assignment.AssignmentRef} Not Checked In Do not send";
                entity.ValidationErrors.Add(message);
                _logger.Warn(message, entity.CorrelationId, rate, entity.ObjectId, "Dtos.Ufo.AssignmentRate", null);

                entity.ExportSuccess = false;
                return;
            }
            
            Rate mappedRate = null;
            try
            {
                mappedRate = rate.MapRate(_rateCodes);
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map assignment {rate.Assignment.AssignmentRef} rate: {exp.Message}", entity.CorrelationId, rate, entity.ObjectId, "AssignmentRate", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }

            SendToRsm(JsonConvert.SerializeObject(mappedRate), Mappers.MapOpCoFromName(rate.Assignment.OpCo.Name).ToString(), "AssignmentRate", entity.CorrelationId, entity.IsCheckedIn);

            _logger.Success($"Successfully sent mapped Assignment {rate.Assignment.AssignmentRef} Rate to RSM", entity.CorrelationId, rate, entity.ObjectId, "Dtos.Ufo.AssignmentRate", null, mappedRate, "Dtos.Sti.AssignmentRate");
            entity.ExportSuccess = true;
        }
    }
}