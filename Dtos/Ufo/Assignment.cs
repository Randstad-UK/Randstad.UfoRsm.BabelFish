using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Randstad.Logging;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish.Dtos.RsmInherited;
using Randstad.UfoRsm.BabelFish.Helpers;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Assignment : ObjectBase
    {
        public Team OpCo { get; set; }

        //required Ids
        public string AssignmentId { get; set; }
        public string MigratedAssignmentId { get; set; }

        //required refs
        public string AssignmentRef { get; set; }
        public string ClientName { get; set; }
        public string PayrollRef { get; set; }
        public string CandidateRef { get; set; }
        public string HleRef { get; set; }
        public string ClientRef { get; set; }
        public string ClientId { get; set; }
        public string JobRef { get; set; }
        public string PositionRef { get; set; }
        public string PositionName { get; set; }
        public string ExternalRef { get; set; }

        //properties
        public string CheckIn { get; set; }
        public bool IR35 { get; set; }
        public string PoNumber { get; set; }
        public bool? PoRequired { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string AssignmentJobTitle { get; set; }
        public string Description { get; set; }
        public string CostCentre { get; set; }
        public string HolidayPay { get; set; }
        public Consultant Owner { get; set; }
        public string InvoiceAddressId { get; set; }
        public InvoiceAddress InvoiceAddress { get; set; }
        public Address WorkAddress { get; set; }
        public string InvoiceContact { get; set; }
        public string Contact { get; set; }
        public string RecordType { get; set; }
        public List<AssignmentRate> Rates { get; set; }
        public List<ConsultantSplit> ConsultantSplits { get; set; }
        public string PaymentType { get; set; }
        public decimal? AwrPreviousWeeks { get; set; }
        public string PreferredPeriod { get; set; }
        public decimal? EnhancedHolidayDays { get; set; }
        public Client Client { get; set; }
        public Candidate Candidate { get; set; }

        public Dtos.RsmInherited.Placement MapAssignment(Dictionary<string, string> tomCodes, ILogger logger, Dictionary<string, string> rateCodes, Guid correlationId)
        {

            var placement = new Dtos.RsmInherited.Placement();
            placement.PAYEDeductionsOnLtdSpecified = true;
            placement.PAYEDeductionsOnLtd = IR35;

            placement.agencyOnlySpecified = true;
            placement.agencyOnly = true;

            placement.bulkEntrySpecified = false;

            //todo: Assignment CIS needs to be pulled once UFO solution specced
            placement.cisApplicableSpecified = true;
            placement.cisApplicable = true;

            placement.client = Client.MapClient();
            placement.consultant = Owner.MapConsultant();

            placement.contractedHoursSpecified = true;
            placement.contractedHours = 40;
            placement.customText1 = ExternalRef;

            placement.endSpecified = true;
            placement.end = EndDate;

            placement.externalId = AssignmentRef;

            placement.faxbackEnabledSpecified = true;
            placement.faxbackEnabled = false;

            placement.holidayAccrualRateSpecified = false;
            placement.holidayAccrualRatePostAWRSpecified = false;

            placement.invoiceRequiresPOSpecified = false;
            if (PoRequired != null)
            {
                placement.invoiceRequiresPOSpecified = true;
                placement.invoiceRequiresPO = PoRequired;
            }

            placement.invoiceContactOverride = InvoiceAddress.MapContact();
            placement.jobDescription = AssignmentJobTitle;

            //todo: Assignment manager needs set to default manager set up in RSM
            placement.manager = new Manager();

            placement.noCommunications = "WMCL";
            placement.permSpecified = true;
            placement.perm = false;

            placement.purchaseBranch = Unit.FinanceCode;
            placement.purchaseDivision = OpCo.Name;
            placement.purchaseOrderNum = PoNumber;

            placement.roundToNearestMinSpecified = true;
            placement.roundToNearestMin = 1;

            placement.salesBranch = Unit.Name;
            placement.salesCostCentre = Unit.FinanceCode;

            placement.siteAddress = WorkAddress.GetAddress();

            MapConsultantSplit(placement);

            placement.start = StartDate;

            placement.timesheetEmailApprovalSpecified = true;
            placement.timesheetEmailApproval = false;
            placement.worker = Candidate.MapWorker(tomCodes, logger, correlationId);

            placement.awrWeekSpecified = false;
            placement.excludeFromMissingTimeSpecified = true;
            placement.excludeFromMissingTime = true;

            MapRates(rateCodes, placement);
            return placement;
        }


        private void MapConsultantSplit(Dtos.RsmInherited.Placement placement)
        {
            if (ConsultantSplits == null || ConsultantSplits.Count <= 1) return;

            placement.splitCommissions = new SplitCommission[ConsultantSplits.Count];
            for (int i = 0; i < ConsultantSplits.Count; i++)
            {
                var split = new Dtos.RsmInherited.ConsultantSplit();
                split.ExternalUserId = ConsultantSplits[i].Consultant.Id;
                split.weight = ConsultantSplits[i].Split;
                placement.consultantSplits[i] = split;
            }

        }

        private void MapRates(Dictionary<string, string> rateCodes, Dtos.RsmInherited.Placement placement)
        {
            if (Rates == null) return;

            placement.rates = new Rate[Rates.Count];


            var priorityOrder = 1;

            //Sti.AssignmentRate stiRate = null;
            foreach (var rate in Rates)
            {
                var rsmRate = rate.MapRate(rateCodes);

                rsmRate.priorityOrderSpecified = true;
                rsmRate.priorityOrder = priorityOrder;
                if (rate.PayUnit == "Hourly")
                {
                    rsmRate.priorityOrder = 0;
                }

                priorityOrder++;
            }

        }
    }
}

