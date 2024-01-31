using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public enum CandidateStatusList { Applicant, Live, Working, DNU, Leaver, Placed, FOJ }

    public class CandidateStatus
    {
        public string Status { get; set; }

    }
}
