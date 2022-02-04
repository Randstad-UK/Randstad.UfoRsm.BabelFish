using System;
using System.Collections.Generic;
using System.Text;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;

namespace Randstad.UfRsm.BabelFish.Dtos.Ufo
{
    public class HolidayRequest
    {
        public Candidate Candidate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string HolidayRequestRef { get; set; }
        public Decimal Hours { get; set; }

        public UfoRsm.BabelFish.Dtos.RsmInherited.HolidayRequest MapHolidayRequest()
        {
            var hr = new UfoRsm.BabelFish.Dtos.RsmInherited.HolidayRequest();

            hr.StartDate = StartDate;
            hr.EndDate = StartDate;
            hr.EmployeeNumber = Candidate.PayrollRefNumber;
            hr.Hours = Hours;
            hr.AbsenceType = 1;
            return hr;
        }
    }
}
