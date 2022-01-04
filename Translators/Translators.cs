using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;

namespace Randstad.UfoRsm.BabelFish
{
    public interface ITranslator
    {
        Task Translate(ExportedEntity entity);
    }
}
