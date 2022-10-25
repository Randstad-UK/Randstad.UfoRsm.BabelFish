using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class ThirdPartyAgency
    {
        public bool IsApproved { get; set; }
        public string AslRef { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public bool? IsCis { get; set; }

    }
}
