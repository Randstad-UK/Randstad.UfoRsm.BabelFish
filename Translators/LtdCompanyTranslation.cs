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


        public LtdCompanyTranslation(IProducerService producer, string baseRoutingKey, ILogger logger, string OpCosToSend, bool allowBlockByDivision) : base(producer, baseRoutingKey, logger, OpCosToSend, allowBlockByDivision)
        {

        }

        public async Task Translate(ExportedEntity entity)
        {
            if (entity.ObjectType != "LtdCompany") return;

            LtdCompany ltd = null;
            try
            {
                ltd = JsonConvert.DeserializeObject<LtdCompany>(entity.Payload);
            }
            catch (Exception exp)
            {
                throw new Exception($"Problem deserialising Ltd Company from UFO {entity.ObjectId} - {exp.Message}");
            }

            _logger.Success($"Recieved Ltd Company {ltd.Name}", entity.CorrelationId, ltd, ltd.Name, "Dtos.Ufo.LtdCompany", null);

            entity.ExportSuccess = true;
        }

    }
}