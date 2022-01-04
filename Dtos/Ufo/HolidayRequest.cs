using System;
using System.Collections.Generic;
using System.Text;
using Randstad.UfoSti.BabelFish.Dtos.Sti;

namespace Randstad.UfoSti.BabelFish.Dtos.Ufo
{
    public class HolidayRequest
    {
        public string EmployerRef { get; set; }
        public Candidate Candidate { get; set; }
        public string WorkerRef { get; set; }
        public DateTime HolidayRequestDate { get; set; }
        public bool? RequestHolidayPay { get; set; }
        public string HolidayRequestRef { get; set; }

        public DateTime? SubmittedOn { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public DateTime? DeclinedOn { get; set; }
        public string Notes { get; set; }
        public Decimal Hours { get; set; }

        public Sti.HolidayRequest MapHolidayRequest(Dictionary<string, string> employerRefs)
        {
            var hr = new Dtos.Sti.HolidayRequest();

            try
            {
                hr.EmployerRef = employerRefs[Candidate.OperatingCo.FinanceCode];
            }
            catch(Exception exp)
            {
                throw new Exception("Problem mapping OpCo for holiday request", exp);
            }
            
            hr.HolidayRequestDate = HolidayRequestDate;
            hr.WorkerRef = Candidate.PayrollRefNumber;
            hr.HolidayRequested = Hours;
            hr.EntityReference = HolidayRequestRef;
            hr.PayAllHoliday = YesNo.N;
            return hr;
        }
    }
}
