using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public abstract class ObjectBase
    {
        public Team Unit { get; set; }
        public Team Branch { get; set; }
        public Team Region { get; set; }
        public Team Division { get; set; }


    }
}