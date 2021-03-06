using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
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

        public CandidateTranslation(IProducerService producer, string baseRoutingKey, Dictionary<string, string> employerRefs, Dictionary<string, string> tomCodes, ILogger logger, string opCosToSend, bool allowBlockByDivision) : base(producer, baseRoutingKey, logger, opCosToSend, allowBlockByDivision)
        {
            _tomCodes = tomCodes;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Candidate") return;

            Candidate candidate = null;

            var liveInPayroll = false;

            try
            {
                candidate = JsonConvert.DeserializeObject<Candidate>(entity.Payload);

                if (candidate.LiveInPayroll == null)
                {
                    _logger.Warn($"Candidate {candidate.CandidateRef} has no live in payroll set (probably because the candidate was created before employee file generated)", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);
                    entity.ExportSuccess = true;
                    return;
                }


                liveInPayroll  = (bool)candidate.LiveInPayroll;
                if (BlockExport(Mappers.MapOpCoFromName(candidate.OperatingCo.Name)))
                {
                    _logger.Warn($"Candidate OpCo not live in RSM {candidate.CandidateRef} {candidate.OperatingCo.Name}", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (BlockExportByDivision(candidate.Division.Name))
                {
                    _logger.Warn($"Candidate Division not live in RSM {candidate.CandidateRef} {candidate.OperatingCo.Name}", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(candidate.OperatingCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {candidate.CandidateRef} Opco", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                //leaver must be sent through only once. The P45 action sets the candidate to leaver and sends out and eventtype of leaver however their liveInPayroll flag will no longer be true
                //so it has to be set here to send to RSM
                if (candidate.Status.Status.ToLower() == "leaver" && entity.EventType.ToLower() == "leaver")
                {
                    liveInPayroll = true;
                }

                //for debug purposes logging a message to show that the candidate is a leaver
                if (candidate.Status.Status.ToLower() == "leaver" && entity.EventType.ToLower() != "leaver")
                {
                    _logger.Warn($"Candidate {candidate.CandidateRef} is at the status {candidate.Status.Status}", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);
                    entity.ExportSuccess = true;
                    return;
                }

            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising Candidate from UFO {entity.ObjectId} - {exp.Message}");
            }

            _logger.Success($"Received Candidate {candidate.CandidateRef}", entity.CorrelationId, candidate, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);

            if (candidate.Status.Status.ToLower() != "live" && candidate.Status.Status.ToLower() != "scheduledforwork" && candidate.Status.Status.ToLower() != "working" && candidate.Status.Status.ToLower() != "leaver" && candidate.Status.Status.ToLower() != "placed")
            {
                _logger.Warn($"Candidate {candidate.CandidateRef} is at the status {candidate.Status.Status}", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);
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

            if (!liveInPayroll)
            {
                _logger.Debug($"Candidate {candidate.CandidateRef} is not live in payroll", entity.CorrelationId, entity, candidate.CandidateRef, "Dtos.Ufo.Candidate", null);
                entity.ExportSuccess = true;
                return;
            }

            SendToRsm(JsonConvert.SerializeObject(rmsWorker), Mappers.MapOpCoFromName(candidate.OperatingCo.Name.ToLower()).ToString(), "Worker", entity.CorrelationId, liveInPayroll);
            _logger.Success($"Successfully mapped Candidate {candidate.CandidateRef} and sent to RSM", entity.CorrelationId, rmsWorker, candidate.CandidateRef, "Dtos.Ufo.Candidate", null, null, "RSM.Worker");
            entity.ExportSuccess = true;
        }
    }
}
