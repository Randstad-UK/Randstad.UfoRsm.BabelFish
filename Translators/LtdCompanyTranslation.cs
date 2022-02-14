using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using Randstad.UfoRsm.BabelFish.Helpers;
using RandstadMessageExchange;

namespace Randstad.UfoRsm.BabelFish.Translators
{
    public class LtdCompanyTranslation : TranslatorBase, ITranslator
    {
        private readonly string _consultantCodePrefix;
        private readonly string _removeFromCandidateRef;
        private readonly Dictionary<string, string> _tomCodes;
        private readonly Dictionary<string, string> _employerRefs;

        public LtdCompanyTranslation(IProducerService producer, string baseRoutingKey, string consultantCodePrefix, string removeFromCandidateRef, Dictionary<string, string> employerRefs, Dictionary<string, string> tomCodes, ILogger logger) : base(producer, baseRoutingKey, logger)
        {
            _consultantCodePrefix = consultantCodePrefix;
            _removeFromCandidateRef = removeFromCandidateRef;
            _tomCodes = tomCodes;
            _employerRefs = employerRefs;
        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "LtdCompany") return;

            LtdCompany ltd= null;
            try
            {
                ltd = JsonConvert.DeserializeObject<LtdCompany>(entity.Payload);

            }
            catch (Exception exp)
            {
                _logger.Warn($"Problem deserialising Ltd Company from UFO {exp.Message}", entity.CorrelationId, entity, ltd.Name, "Dtos.Ufo.ExportedEntity", null);
                entity.ExportSuccess = false;
                return;
            }

            _logger.Success($"Recieved Ltd Company {ltd.Name}", entity.CorrelationId, ltd, ltd.Name, "Dtos.Ufo.LtdCompany", null);


            
            //try
            //{
            //    foreach (var c in ltd.Candidates)
            //    {

            //        if (string.IsNullOrEmpty(c.OperatingCo.FinanceCode))
            //        {
            //            _logger.Warn($"No Finance Code On {c.CandidateRef} Opco", entity.CorrelationId, entity, entity.ObjectId, "Dtos.Ufo.ExportedEntity", null);
            //            continue;
            //        }


            //        var supplier = ltd.GetSupplier(c.PaymentMethod);

            //        try
            //        {
            //            supplier.Department = c.Unit.FinanceCode;
            //        }
            //        catch (Exception exp)
            //        {
            //            throw new Exception($"Problem mapping Department for ltd company worker {c.CandidateRef}", exp);
            //        }

            //        try
            //        {
            //            supplier.Division = _tomCodes[c.Unit.FinanceCode];
            //        }
            //        catch (Exception exp)
            //        {
            //            throw new Exception($"Problem mapping Division for ltd company worker {c.CandidateRef}", exp);
            //        }

            //        try
            //        {
            //            supplier.EmployerRef = _employerRefs[c.OperatingCo.FinanceCode];
            //        }
            //        catch (Exception exp)
            //        {
            //            throw new Exception($"Problem mapping Employer Ref for ltd company worker {c.CandidateRef}", exp);
            //        }


            //        try
            //        {
            //            supplier.OpCo = Mappers.MapOpCo(c.OperatingCo.FinanceCode);
            //        }
            //        catch (Exception exp)
            //        {
            //            throw new Exception($"Problem mapping OpCo for ltd company worker {c.CandidateRef}", exp);
            //        }

                    
            //        supplier.SupplierRef = c.PayrollRefNumber;

            //        //Default none CPE Construction Industry Scheme) fields as it is possible in UFO for them to select opt in when not relevent to sector
            //        if (c.OperatingCo.FinanceCode != "CPE")
            //        {
            //            supplier.LegalStatus = SupplierLegalStatus.L;
            //            supplier.CISLegalStatus = null;
            //        }


            //        SendToSti(JsonConvert.SerializeObject(supplier), supplier.OpCo.ToString(), "Supplier", entity.CorrelationId, true);

            //        _logger.Success($"Successfully mapped supplier {supplier.SupplierRef} for Candidate {c.CandidateRef} and sent to STI",entity.CorrelationId, supplier, entity.ObjectId, "Dtos.Ufo.LtdCompany", null,supplier, "Dtos.Sti.Supplier");
            //    }

            //}
            //catch (Exception exp)
            //{
            //    if (entity.ValidationErrors == null)
            //        entity.ValidationErrors = new List<string>();

            //    _logger.Warn($"Failed to map supplier: {exp.Message}", entity.CorrelationId, ltd, entity.ObjectId, "LtdCompany", null);
            //    entity.ValidationErrors.Add(exp.Message);
            //    entity.ExportSuccess = false;
            //    return;

            //}

            
            entity.ExportSuccess = true;
        }
    
    }
}
