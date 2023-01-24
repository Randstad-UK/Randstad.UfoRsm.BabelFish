using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class ConsultantTranslation : TranslatorBase, ITranslator
    {

        public ConsultantTranslation(IProducerService producer, string routingKeyBase, ILogger logger, string opCosToSend, bool allowBlockByDivision) : base(producer, routingKeyBase, logger, opCosToSend, allowBlockByDivision)
        {

        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Consultant" && entity.ObjectType != "User") return;

            Dtos.Ufo.Consultant consultant;
            try
            {
                consultant = JsonConvert.DeserializeObject<Dtos.Ufo.Consultant>(entity.Payload);

                if (consultant.OpCo == null)
                {
                    _logger.Warn($"Consultant not correctly set up in UFO (Probably no primary unit) {consultant.EmployeeRef}", entity.CorrelationId, entity, consultant.EmployeeRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(consultant.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {consultant.EmployeeRef} Opco", entity.CorrelationId, entity, consultant.EmployeeRef, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising Consultant from UFO {entity.ObjectId} - {exp.Message}");
            }

            _logger.Success($"Received Consultant {consultant.EmployeeRef}", entity.CorrelationId, consultant, consultant.EmployeeRef, "Dtos.Ufo.Consultant", null);

            RSM.Consultant rsmConsultant = null;
            try
            {
                rsmConsultant = consultant.MapConsultant();
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map consultant {consultant.EmployeeRef}: {exp.Message}", entity.CorrelationId, consultant, consultant.EmployeeRef, "Dtos.Ufo.Consultant", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }

            var opco = Mappers.MapOpCoFromName(consultant.OpCo.Name).ToString();

            //send the consultant
            SendToRsm(JsonConvert.SerializeObject(rsmConsultant), opco, "consultant", entity.CorrelationId, true);
            _logger.Success($"Successfully sent consultant {consultant.EmployeeRef} to RSM", entity.CorrelationId, rsmConsultant, consultant.EmployeeRef, "Dtos.Ufo.Consultant", null, null, "RSM.Consultant");


            entity.ExportSuccess = true;
        }
    }
}
