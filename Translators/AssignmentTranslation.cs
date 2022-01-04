using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoSti.BabelFish.Dtos.Sti;
using Randstad.UfoSti.BabelFish.Dtos.Ufo;
using RandstadMessageExchange;
using Assignment = Randstad.UfoSti.BabelFish.Dtos.Ufo.Assignment;

namespace Randstad.UfoSti.BabelFish.Translators
{
    public class AssignmentTranslation : TranslatorBase, ITranslator
    {
        private readonly Dictionary<string, string> _rateCodes;
        private readonly string _consultantCodePrefix;
        private readonly Dictionary<string, string> _tomCodes;
        private readonly Dictionary<string, string> _employerRefs;

        public AssignmentTranslation(Dictionary<string, string> rateCodes, IProducerService producer, string routingKeyBase, string consultantCodePrefix, Dictionary<string, string> tomCodes, Dictionary<string, string> employerRefs, ILogger logger) : base(producer, routingKeyBase, logger)
        {
            _consultantCodePrefix = consultantCodePrefix;
            _rateCodes = rateCodes;
            _employerRefs = employerRefs;
            _tomCodes = tomCodes;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "Assignment") return;

            Assignment assign = null;
            try
            {
                assign = JsonConvert.DeserializeObject<Dtos.Ufo.Assignment>(entity.Payload);

                if (string.IsNullOrEmpty(assign.OpCo.FinanceCode))
                {
                    _logger.Warn($"No Finance Code On {assign.AssignmentRef} Opco", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

                if (string.IsNullOrEmpty(assign.PreferredPeriod))
                {
                    _logger.Warn($"Assignment {assign.AssignmentRef} is historic should not export", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                    entity.ExportSuccess = false;
                    return;
                }

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Assignment from UFO {exp.Message}", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Assignment {assign.AssignmentRef}", entity.CorrelationId, assign, entity.ObjectId, "Dtos.Ufo.Assignment", null);

            if (string.IsNullOrEmpty(assign.CheckIn) || assign.CheckIn!="Checked In")
            {
                if(entity.ValidationErrors==null)
                    entity.ValidationErrors = new List<string>();

                var message = $"Assignment {assign.AssignmentRef} is not checked in";
                entity.ValidationErrors.Add(message);

                _logger.Warn(message, entity.CorrelationId, assign, entity.ObjectId, "Dtos.Ufo.Assignment", null);
                entity.ExportSuccess = false;
                return;
                
            }

            List<Dtos.Sti.AssignmentRate> rates = null;
            Dtos.Sti.Assignment assignment = null;
            
            ClientAddress invoiceAddress = null;
            try
            {
                assignment = assign.MapAssignment(_rateCodes, out rates, _consultantCodePrefix, _tomCodes, _employerRefs, out invoiceAddress);

            }
            catch (Exception exp)
            {
                if (entity.ValidationErrors == null)
                    entity.ValidationErrors = new List<string>();

                _logger.Warn($"Failed to map assignment {assign.AssignmentRef}: {exp.Message}", entity.CorrelationId, assignment, entity.ObjectId, "Assignment", null);
                entity.ValidationErrors.Add(exp.Message);
                entity.ExportSuccess = false;

                return;
            }

            if (invoiceAddress != null)
            {
                SendClientAddressToOpcos(invoiceAddress, entity.CorrelationId, assign.InvoiceAddress, entity.ObjectId, assignment.AssignmentRef);
            }

            SendToSti(JsonConvert.SerializeObject(assignment), assignment.OpCo.ToString(), "Assignment", entity.CorrelationId, (bool)assignment.IsStartChecked);

            _logger.Success($"Successfully mapped Assignment {assignment.AssignmentRef} and sent to STI", entity.CorrelationId, assign, entity.ObjectId, "Dtos.Ufo.Assignment", null, assignment, "Dtos.Sti.Assignment");
            if (rates == null)
            {
                return;
            }

            foreach(var rate in rates)
            {
                SendToSti(JsonConvert.SerializeObject(rate), assignment.OpCo.ToString(), "AssignmentRate", entity.CorrelationId, (bool)assignment.IsStartChecked);
                _logger.Success($"Successfully mapped Rate for Assignment {rate.AssignmentRef} and sent to STI", entity.CorrelationId, rate, null, "Dtos.Ufo.AssignmentRate", null);
            }

            entity.ExportSuccess = true;
        }

        private void SendClientAddressToOpcos(ClientAddress clientAddresses, Guid correlationId,
            Dtos.Ufo.InvoiceAddress ufoInvoiceAddress, string entityId, string assignmentRef)
        {

            foreach (var opco in _employerRefs)
            {
                var o = opco.Key;
                clientAddresses.EmployerRef = opco.Value;

                if (o.ToLower() == "ps")
                {
                    o = "care";
                }

                //Is Checked in always true as UFO won't export unless it is
                SendToSti(JsonConvert.SerializeObject(clientAddresses), o, "ClientAddress", correlationId, true);
                _logger.Success(
                    $"Successfully mapped ClientAddress {ufoInvoiceAddress.InvoiceAddressRef} for Assignment {assignmentRef} and Sent To {o} STI",
                    correlationId, ufoInvoiceAddress, ufoInvoiceAddress.AddressId, "Dtos.Ufo.InvoiceAddress", null,
                    clientAddresses, "Dtos.Sti.ClientAddress");
            }
        }


    }
}
