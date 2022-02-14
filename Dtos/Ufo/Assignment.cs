using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Randstad.Logging;
using Randstad.Logging.Core;
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
        public ClientContact InvoicePerson { get; set; }
        public string Contact { get; set; }
        public string RecordType { get; set; }
        public List<AssignmentRate> Rates { get; set; }
        public List<ConsultantSplit> ConsultantSplits { get; set; }
        public string PaymentType { get; set; }
        public decimal? AwrPreviousWeeks { get; set; }
        public string PreferredPeriod { get; set; }
        public decimal? EnhancedHolidayDays { get; set; }
        public Client Client { get; set; }
        public Client Hle { get; set; }
        public Candidate Candidate { get; set; }

        public Dtos.RsmInherited.Placement MapAssignment(Dictionary<string, string> tomCodes, ILogger logger, Dictionary<string, string> rateCodes, Guid correlationId)
        {

            var placement = new Dtos.RsmInherited.Placement();
            placement.PAYEDeductionsOnLtdSpecified = true;
            placement.PAYEDeductionsOnLtd = IR35;

            placement.agencyOnlySpecified = true;
            placement.agencyOnly = true;

            placement.bulkEntrySpecified = false;

            //TODO: Assignment CIS needs to be pulled once UFO solution specced
            placement.cisApplicableSpecified = false;

            placement.client = Hle.MapClient();
            placement.consultant = Owner.MapConsultant();

            placement.contractedHoursSpecified = true;
            placement.contractedHours = 40;

            placement.endSpecified = true;
            placement.end = EndDate;

            placement.expenseEmailApprovalSpecified = true;
            placement.expenseEmailApproval = false;

            placement.externalId = AssignmentRef;

            placement.faxbackEnabledSpecified = true;
            placement.faxbackEnabled = false;

            placement.holidayAccrualRateSpecified = false;
            
            if (EnhancedHolidayDays != null)
            {
                placement.holidayAccrualRateSpecified = true;
                var perc = Math.Round((decimal)EnhancedHolidayDays / (260 - (decimal)EnhancedHolidayDays), 4, MidpointRounding.AwayFromZero);
                placement.holidayAccrualRate = perc;
            }

            placement.holidayAccrualRatePostAWRSpecified = false;

            placement.invoiceRequiresPOSpecified = false;
            if (PoRequired != null)
            {
                placement.invoiceRequiresPOSpecified = true;
                placement.invoiceRequiresPO = PoRequired;
            }

            placement.expenseTemplate = "Standard Expenses";
            placement.invoiceContactOverride = InvoicePerson.MapContact();

            var invoiceEmailList = new List<string>();
            if (!string.IsNullOrEmpty(Client.InvoiceEmail))
            {
                invoiceEmailList.Add(Client.InvoiceEmail);
            }

            if (!string.IsNullOrEmpty(Client.InvoiceEmail2))
            {
                invoiceEmailList.Add(Client.InvoiceEmail2);
            }

            if (!string.IsNullOrEmpty(Client.InvoiceEmail3))
            {
                invoiceEmailList.Add(Client.InvoiceEmail3);
            }

            foreach (var email in invoiceEmailList)
            {
                placement.invoiceContactOverride.email = placement.invoiceContactOverride.email + email + "; ";
            }

            if (!string.IsNullOrEmpty(placement.invoiceContactOverride.email) && placement.invoiceContactOverride.email.EndsWith(";"))
            {
                placement.invoiceContactOverride.email.Remove(placement.invoiceContactOverride.email.Length, 1);
            }

            placement.invoiceContactOverride.address = InvoiceAddress.MapAddress();

            placement.jobTitle = string.IsNullOrEmpty(PositionName) ? "Not Stated" : PositionName;

            placement.jobDescription = AssignmentJobTitle;

            placement.noCommunications = "WMCL";
            placement.permSpecified = true;
            placement.perm = false;

            placement.purchaseBranch = Unit.Name;
            placement.purchaseCostCentre = Unit.FinanceCode;
            placement.purchaseDivision = OpCo.Name;
            placement.purchaseOrderNum = PoNumber;

            placement.roundToNearestMinSpecified = true;
            placement.roundToNearestMin = 1;

            placement.salesBranch = Unit.Name;
            placement.salesCostCentre = Unit.FinanceCode;
            placement.salesDivision = OpCo.Name;

            placement.siteAddress = WorkAddress.GetAddress();

            MapConsultantSplit(placement);

            placement.startSpecified = true;
            placement.start = StartDate;

            placement.timesheetEmailApprovalSpecified = true;
            placement.timesheetEmailApproval = false;
            placement.worker = Candidate.MapWorker(tomCodes, logger, correlationId);

            placement.awrWeekSpecified = false;
            placement.excludeFromMissingTimeSpecified = true;
            placement.excludeFromMissingTime = true;

            placement.customText1 = AssignmentRef;
            placement.customText2 = Client.ClientRef;

            if (WorkAddress != null)
            {
                placement.customText3 = WorkAddress.Street + ", ";

                if (!string.IsNullOrEmpty(WorkAddress.City))
                {
                    placement.customText3 = placement.customText3 + WorkAddress.City + ", ";
                }

                if (!string.IsNullOrEmpty(WorkAddress.County))
                {
                    placement.customText3 = placement.customText3 + WorkAddress.County + ", ";
                }

                if (!string.IsNullOrEmpty(WorkAddress.Country))
                {
                    placement.customText3 = placement.customText3 + WorkAddress.Country + ", ";
                }

                if (!string.IsNullOrEmpty(WorkAddress.PostCode))
                {
                    placement.customText3 = placement.customText3 + WorkAddress.PostCode;
                }

                if (placement.customText3.EndsWith(", "))
                {
                    placement.customText3.Remove(placement.customText3.LastIndexOf(", "));
                }
            }
            
            placement.clientSite = Client.ClientName + ", " + Client.WorkAddress.Street+", "+Client.WorkAddress.City+", "+Client.WorkAddress.County+", "+Client.WorkAddress.PostCode;

            MapRates(rateCodes, placement);

            //TODO: (Done) Assignment manager needs set to default manager set up in RSM
            placement.manager = Mappers.GetDefaultManager();

            placement.timesheetApprovalRoute = "Auto Approval Route";
            MapLtdValues(placement);

            return placement;
        }

        private void MapLtdValues(RSM.Placement placement)
        {
            if (Candidate.PayType != PaymentTypes.LTD) return;

            placement.cisApplicable = Mappers.MapBool(Candidate.LtdCompany.Cis);
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
                placement.splitCommissions[i] = split;
            }

        }

        private void MapRates(Dictionary<string, string> rateCodes, Dtos.RsmInherited.Placement placement)
        {
            if (Rates == null) return;

            placement.rates = new RSM.Rate[Rates.Count];


            var priorityOrder = 1;
            var rateIndex = 0;

            foreach (var rate in Rates)
            {
                var rsmRate = rate.MapRate(rateCodes, this);

                rsmRate.priorityOrderSpecified = true;
                rsmRate.priorityOrder = priorityOrder;


                if (rate.PayUnit == "Hourly")
                {
                    rsmRate.priorityOrder = 0;
                }
                
                priorityOrder++;
                placement.rates[rateIndex]=rsmRate;
                rateIndex++;
            }

        }
    }
}

