using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class CandidateTranslation : TranslatorBase, ITranslator
    {
        private readonly Dictionary<string, string> _tomCodes;

        public CandidateTranslation(IProducerService producer, string baseRoutingKey, Dictionary<string, string> employerRefs, Dictionary<string, string> tomCodes, ILogger logger) : base(producer, baseRoutingKey, logger)
        {
            _tomCodes = tomCodes;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Candidate") return;

            Candidate candidate = null;
            try
            {
                candidate = JsonConvert.DeserializeObject<Candidate>(entity.Payload);

                if (BlockExport(Mappers.MapOpCoFromName(candidate.OperatingCo.Name)))
                {
                    _logger.Warn($"Candidate OpCo not live in RSM {candidate.CandidateRef} {candidate.OperatingCo.Name}", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(candidate.OperatingCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {candidate.CandidateRef} Opco", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (candidate.LiveInPayroll == null || candidate.LiveInPayroll==false)
                {
                    _logger.Warn($"Candidate is not live in payroll {candidate.CandidateRef}", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Candidate from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Candidate {candidate.CandidateRef}", entity.CorrelationId, candidate, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);

            if (candidate.Status.Status.ToLower() != "live" && candidate.Status.Status.ToLower() != "scheduledforwork" && candidate.Status.Status.ToLower() != "working" && candidate.Status.Status.ToLower() != "leaver" && candidate.Status.Status.ToLower() != "placed")
            {
                _logger.Debug($"Candidate{candidate.CandidateRef} is at the status {candidate.Status}", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);
                entity.ExportSuccess = false;
                return;
            }

            if (string.IsNullOrEmpty(candidate.PaymentMethod) && (candidate.PayType==PaymentTypes.LTD || candidate.PayType==PaymentTypes.PAYE))
            {
                if (entity.ValidationErrors == null) 
                    entity.ValidationErrors=new List<string>();

                 var message = $"Candidate {candidate.CandidateRef} has no payment method";
                 entity.ValidationErrors.Add(message);

                _logger.Warn(message, entity.CorrelationId, candidate, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);
                entity.ExportSuccess = false;
                return;
            }



            var liveInPayroll = (bool)candidate.LiveInPayroll;
            if (candidate.Status.Status.ToLower() == "leaver")
            {
                liveInPayroll = true;
            }


            RSM.Worker rmsWorker = null;

            try
            {
                rmsWorker = candidate.MapWorker(_tomCodes, _logger, entity.CorrelationId);
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map candidate: {exp.Message}", entity.CorrelationId, candidate, entity.ObjectId, "Candidate", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
                
            }

            SendToRsm(JsonConvert.SerializeObject(rmsWorker), Mappers.MapOpCoFromName(candidate.OperatingCo.Name.ToLower()).ToString(), "Worker", entity.CorrelationId, liveInPayroll);
            _logger.Success($"Successfully mapped Candidate {candidate.CandidateRef} and sent to RSM", entity.CorrelationId, candidate, candidate.CandidateRef, "Dtos.Ufo.Candidate", null, rmsWorker, "RSM.Worker");
            entity.ExportSuccess = true;
        }
    }
}
