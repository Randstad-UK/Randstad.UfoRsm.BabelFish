using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;

namespace Randstad.UfoSti.BabelFish.Dtos.Ufo
{
    public class CandidateDivision
    {
        public Candidate Candidate { get; set; }
        public string CandidateDivisionId { get; set; }
        public Division Division { get; set; }
        public string TempStatus { get; set; }

    }
}
