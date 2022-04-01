using System;
using System.Collections.Generic;
using System.Text;
using Randstad.UfoRsm.BabelFish.Dtos;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class TimesheetLine
    {
        public string PoNumber { get; set; }
        public AssignmentRate Rate { get; set; }
        public decimal? TotalHours { get; set; }
        public decimal? TotalDays { get; set; }
        public decimal? HoursReported { get; set; }
        public decimal? DaysReported { get; set; }
        public bool IsMapped { get; set; }
        public decimal? BreakTimeMinutes { get; set; }
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string HoursType { get; set; }
        public DateTime? BreakStartTime { get; set; }
        public DateTime? BreakEndTime { get; set; }


    }
}
