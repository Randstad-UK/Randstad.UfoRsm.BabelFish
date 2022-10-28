using System;
using System.Collections.Generic;
using System.Text;
using Randstad.UfoRsm.BabelFish.Dtos;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class TimesheetLine
    {
        public AssignmentRate Rate { get; set; }
        public decimal? TotalHours { get; set; }

        public decimal? DaysReported { get; set; }

        public string StartDateTime { get; set; }
        public string EndDateTime { get; set; }
        public string HoursType { get; set; }
        public string BreakStartTime { get; set; }
        public string BreakEndTime { get; set; }
        public string PoNumber { get; set; }


    }
}
