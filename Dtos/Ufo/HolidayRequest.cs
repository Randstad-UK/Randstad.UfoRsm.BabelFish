using System;
using System.Collections.Generic;
using System.Text;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Dtos;
using Randstad.UfoRsm.BabelFish.Dtos.Ufo;
using RSM;

namespace Randstad.UfRsm.BabelFish.Dtos.Ufo
{
    public class HolidayRequest
    {
        public Candidate Candidate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string HolidayRequestRef { get; set; }
        public Decimal Hours { get; set; }
        public string ApproverExternalId { get; set; }


        public RSM.HolidayClaim MapHolidayRequest(List<DivisionCode> divisionCodes, ILogger logger, Guid correlationId)
        {


            var hr = new RSM.HolidayClaim();
            hr.worker = Candidate.MapWorker(divisionCodes, logger, correlationId);

            hr.daysClaimedSpecified = true;
            hr.daysClaimed = Hours;
            
            hr.periodStartDateSpecified = true;
            hr.periodStartDate = StartDate;

            hr.periodEndDateSpecified = true;
            hr.periodEndDate = EndDate;
            hr.showIn = "hours";
            hr.showInString = "hours";
            
            hr.submittedSpecified = true;
            hr.submitter = new User(){externalId = Candidate.CandidateRef};

            hr.approver = new User();
            hr.approver.externalId = ApproverExternalId;
            
            return hr;
        }
    }
}
