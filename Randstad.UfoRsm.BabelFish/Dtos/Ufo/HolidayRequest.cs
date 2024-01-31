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
        public string HolidayRequestId { get; set; }
        public string HolidayRequestRef { get; set; }
        public Candidate Candidate { get; set; }
        public DateTime HolidayRequestDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime ApprovedOn { get; set; }
        public DateTime DeclinedOn { get; set; }
        public string Notes { get; set; }
        public Decimal Hours { get; set; }
        public string ApproverExternalId { get; set; }


        public RSM.HolidayClaim MapHolidayRequest(List<DivisionCode> divisionCodes, ILogger logger, Guid correlationId)
        {


            var hr = new RSM.HolidayClaim();
            hr.worker = Candidate.MapWorker(divisionCodes, logger, correlationId);

            hr.daysClaimedSpecified = true;
            hr.daysClaimed = Hours;
            hr.submittedSpecified = true;
            hr.submitted = SubmittedOn;
            hr.approvedSpecified = true;
            hr.approved = ApprovedOn;
            hr.approver = new User();
            hr.approver.externalId = "UFO" + ApproverExternalId;
            hr.submitter = new User();
            hr.submitter.externalId = "UFO" + ApproverExternalId;

            hr.periodStartDateSpecified = true;
            hr.periodStartDate = StartDate;

            hr.periodEndDateSpecified = true;
            hr.periodEndDate = EndDate;
            hr.showIn = "hours";
            hr.showInString = "hours";

            hr.comment = HolidayRequestRef;
            hr.recordStatus = "Approved";

            return hr;
        }
    }
}