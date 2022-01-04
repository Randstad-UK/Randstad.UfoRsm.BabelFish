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

        public ConsultantTranslation(IProducerService producer, string routingKeyBase, ILogger logger) : base(producer, routingKeyBase, logger)
        {

        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Consultant") return;

            Dtos.Ufo.Consultant consultant;
            try
            {
                consultant = JsonConvert.DeserializeObject<Dtos.Ufo.Consultant>(entity.Payload);


                if (string.IsNullOrEmpty(consultant.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {consultant.EmployeeRef} Opco", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Consultant from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Consultant {consultant.EmployeeRef}", entity.CorrelationId, consultant, entity.ObjectId, "Dtos.Ufo.Consultant", null);

            RSM.Consultant rsmConsultant = null;
            try
            {
                rsmConsultant = consultant.MapConsultant();
            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map consultant {consultant.EmployeeRef}: {exp.Message}", entity.CorrelationId, consultant, entity.ObjectId, "Dtos.Ufo.Consultant", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;
                return;
            }

            var opco = Mappers.MapOpCoFromName(consultant.OpCo.Name).ToString();

            //send the consultant
            SendToRsm(JsonConvert.SerializeObject(rsmConsultant), opco, "consultant", entity.CorrelationId, true);
            _logger.Success($"Successfully sent consultant {consultant.EmployeeRef} to RSM", entity.CorrelationId, consultant, entity.ObjectId, "Dtos.Ufo.Consultant", null, rsmConsultant, "RSM.Consultant");

        
            entity.ExportSuccess = true;
        }
    }
}
