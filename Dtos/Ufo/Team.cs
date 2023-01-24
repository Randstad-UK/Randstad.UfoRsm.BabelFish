using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Team
    {
        public string Id { get; set; }
        public string MigratedGroupId { get; set; }
        public bool? IsActive { get; set; }
        public string Name { get; set; }
        public string FinanceCode { get; set; }
    }
}