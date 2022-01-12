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

        

        //public string WorkArea { get; set; }
        //public string SpecialWorkCondition { get; set; }
        //public string Status { get; set; }
        //public string Shift { get; set; }
        //public string Scheduled { get; set; }
        //
        //public decimal? WageRate { get; set; }
        //public decimal? HoursReported { get; set; }

        //public decimal? UnscheduledHours { get; set; }
        //public decimal? OvertimeHours { get; set; }
        //public decimal? Milage { get; set; }

        //public string Remarks { get; set; }


        //public decimal? TotalSeconds { get; set; }
        //public decimal? BreakHours { get; set; }
        //public DateTime? TimesheetLineDate { get; set; }

        //public DateTime?ScheduledBreakStartTime { get; set; }
        //public DateTime? ScheduledBreakEndTime { get; set; }
        //public DateTime? BreakStartTime { get; set; }
        //public DateTime? BreakEndTime { get; set; }
        //public bool? OutOfSchedule { get; set; }
        //public bool? MultipleBreaks { get; set; }
        //public bool? LateNotified { get; set; }
        //public string TimesheetLineRef { get; set; }

    }
}
