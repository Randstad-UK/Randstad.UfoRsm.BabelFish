using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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

        public AssignmentRateTranslation(Dictionary<string, string> rateCodes, IProducerService producer, string routingKeyBase, ILogger logger, string opCosToSend, bool allowBlockByDivision) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
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
                    _logger.Warn($"Assignment OpCo not live in RSM for assignment {rate.Assignment.AssignmentRef} {rate.Assignment.OpCo.Name}", entity.CorrelationId, entity, rate.FeeRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (BlockExportByDivision(rate.Assignment.Division.Name))
                {
                    _logger.Warn($"Assignment Division not live in RSM for assignment {rate.Assignment.AssignmentRef} {rate.Assignment.Division.Name}", entity.CorrelationId, entity, rate.FeeRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(rate.Assignment.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {rate.FeeRef} Opco", entity.CorrelationId, entity, rate.FeeRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(rate.Assignment.PreferredPeriod))
                {
                    _logger.Warn($"Assignment Rate {rate.FeeRef} attached assignment {rate.Assignment.AssignmentRef} is historic should not export", entity.CorrelationId, entity, rate.FeeRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (rate.RateType == "Expense Rate" && rate.FeeName!= "Bonus" && rate.FeeName!= "Back Pay - Non WTR" && rate.FeeName!= "Back Pay - WTR")
                {
                    _logger.Warn($"Assignment Rate {rate.FeeRef} attached assignment {rate.Assignment.AssignmentRef} is an Expense Type which does not get sent to RSM", entity.CorrelationId, entity, rate.FeeRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising AssignmentRate from UFO {entity.ObjectId} - {exp.Message}");
            }

            _logger.Success($"Received Assignment Rate {rate.FeeRef} for Assignment {rate.Assignment.AssignmentRef}", entity.CorrelationId, rate, rate.FeeRef, "Dtos.Ufo.AssignmentRate", null);

            if (string.IsNullOrEmpty(rate.Assignment.CheckIn) || rate.Assignment.CheckIn != "Checked In")
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Rate Assignment {rate.Assignment.AssignmentRef} for Rate {rate.FeeRef} Not Checked In Do not send";
                entity.ValidationErrors.Add(message);
                _logger.Debug(message, entity.CorrelationId, rate, rate.FeeRef, "Dtos.Ufo.AssignmentRate", null);

                entity.ExportSuccess = false;
                return;
            }
            
            RSM.Rate mappedRate = null;
            RSM.Rate mappedPostRate = null;
            try
            {
                mappedRate = rate.MapRate(_rateCodes, out mappedPostRate);
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map assignment {rate.FeeRef}  rate: {exp.Message}", entity.CorrelationId, rate, rate.FeeRef, "Rate", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }

            SendToRsm(JsonConvert.SerializeObject(mappedRate), Mappers.MapOpCoFromName(rate.Assignment.OpCo.Name).ToString(), "Rate", entity.CorrelationId, entity.IsCheckedIn);

            _logger.Success($"Successfully sent mapped Assignment Rate {rate.FeeRef} to RSM", entity.CorrelationId, mappedRate, rate.FeeRef, "Dtos.Ufo.AssignmentRate", null, null, "Dtos.Sti.AssignmentRate");

            if (mappedPostRate != null)
            {
                SendToRsm(JsonConvert.SerializeObject(mappedPostRate), Mappers.MapOpCoFromName(rate.Assignment.OpCo.Name).ToString(), "Rate", entity.CorrelationId,entity.IsCheckedIn);
                _logger.Success($"Successfully sent mapped post parity Assignment Rate {rate.FeeRef} to RSM", entity.CorrelationId, mappedPostRate, rate.FeeRef, "Dtos.Ufo.AssignmentRate", null, null, "Dtos.Sti.AssignmentRate");
            }

            entity.ExportSuccess = true;
        }
    }
}
