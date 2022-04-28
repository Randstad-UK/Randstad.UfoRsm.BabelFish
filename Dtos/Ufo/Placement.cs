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
    public class Placement : ObjectBase
    {
        public Team Unit { get; set; }
        public Team OpCo { get; set; }

        public string PlacementRef { get; set; }
        public string PoNumber { get; set; }
        public string PoRequired { get; set; }
        public DateTime StartDate { get; set; }
        public string PlacementJobTitle { get; set; }

        public string CandidateFirstName { get; set; }
        public string CandidateLastName { get; set; }

        public decimal Fee { get; set; }
        public string CheckIn { get; set; }
        public bool IR35 { get; set; }
        public string CostCentre { get; set; }

        public Owner Owner { get; set; }
        public InvoiceAddress InvoiceAddress { get; set; }
        public ClientContact InvoicePerson { get; set; }


        public Client RsmClient { get; set; }
        public Client Hle { get; set; }

        public List<ConsultantSplit> ConsultantSplits { get; set; }

        public Dtos.RsmInherited.Placement MapPlacement(Dictionary<string, string> tomCodes, ILogger logger, Guid correlationId)
        {

            var placement = new Dtos.RsmInherited.Placement();
            placement.holidayAccrualRateSpecified = false;
            placement.invoiceRequiresPOSpecified = false;
            placement.bulkEntrySpecified = false;
            placement.holidayAccrualRatePostAWRSpecified = false;
            placement.startSpecified = true;
            placement.start = StartDate.ToLocalTime();

            placement.awrWeekSpecified = false;
            placement.excludeFromMissingTimeSpecified = true;
            placement.excludeFromMissingTime = false;

            placement.PAYEDeductionsOnLtdSpecified = true;
            placement.PAYEDeductionsOnLtd = false;

            placement.agencyOnlySpecified = true;
            placement.agencyOnly = true;

            placement.cisApplicableSpecified = true;
            placement.cisApplicable = false;

            placement.client = Hle.MapClient();
            placement.clientSite = RsmClient.ClientName;

            placement.consultant = Owner.MapConsultant();
            placement.customText2 = RsmClient.ClientRef;
            placement.customText3 = RsmClient.WorkAddress.GetConcatenatedAddress();

            placement.endSpecified = true;
            placement.end = DateTime.Now;

            placement.expenseEmailApprovalSpecified = true;
            placement.expenseEmailApproval = false;

            placement.externalId = PlacementRef;

            placement.faxbackEnabledSpecified = true;
            placement.faxbackEnabled = false;

            placement.contractedHoursSpecified = true;
            placement.contractedHours = 0;

            if (CostCentre == null)
            {
                CostCentre = "";
            }
            placement.customText4 = CostCentre;

            var placementStartDate = (DateTime) StartDate;
            
            //TODO: Remove this once fix confirmed
            //placement.chargeTermsExtraTextOverride = $"For the permanent placement of {CandidateFirstName} {CandidateLastName}, {PlacementJobTitle}, {placementStartDate.ToString("dd/MM/yyyy")}, placement reference {PlacementRef}";

            

            placement.invoiceRequiresPOSpecified = false;
            if (PoRequired != null)
            {
                placement.invoiceRequiresPOSpecified = true;
                placement.invoiceRequiresPO = Mappers.MapBool(PoRequired);
                placement.purchaseOrderNum = PoNumber;
            }

            placement.invoiceContactOverride = InvoicePerson.MapContact();

            //Billing team do not want the name values
            placement.invoiceContactOverride.firstname = string.Empty;
            placement.invoiceContactOverride.lastname = string.Empty;

            placement.invoiceContactOverride.address = InvoiceAddress.MapAddress();
            placement.jobDescription = PlacementJobTitle;
            placement.jobTitle = PlacementJobTitle;

            placement.noCommunications = "WMCL";
            placement.permSpecified = true;
            placement.perm = true;

            placement.purchaseBranch = Unit.Name;
            placement.purchaseCostCentre = Unit.FinanceCode;
            placement.purchaseDivision = OpCo.Name;
            placement.purchaseOrderNum = PoNumber;
            placement.roundToNearestMinSpecified = true;
            placement.roundToNearestMin = 1;
            placement.salesDivision = OpCo.Name;
            placement.siteAddress = RsmClient.WorkAddress.GetAddress();
            placement.salesBranch = Unit.Name;
            placement.salesCostCentre = Unit.FinanceCode;

            placement.PAYEDeductionsOnLtdSpecified = true;
            placement.PAYEDeductionsOnLtd = IR35;

            MapConsultantSplit(placement);

            placement.timesheetApprovalRoute = "Auto Approval Route";

            placement.timesheetEmailApprovalSpecified = true;
            placement.timesheetEmailApproval = false;

            placement.rates = new RSM.Rate[1];

            var rate = new RSM.Rate();
            rate.awrSpecified = true;
            rate.awrSpecified = false;
            rate.backendRef = PlacementRef;
            rate.chargeSpecified = true;
            rate.charge = Fee;
            rate.paySpecified = true;
            rate.pay = 0;
            rate.ExternalAssignmentRef = PlacementRef;
            rate.effectiveFromSpecified = true;
            rate.effectiveFrom = StartDate.ToLocalTime();
            rate.frontendRef = PlacementRef;
            
            //TODO: remove once confirmed that fix is working.
            //rate.name = "Perm Placement Fee";

            rate.name = placement.chargeTermsExtraTextOverride = $"For the permanent placement of {CandidateFirstName} {CandidateLastName}, {PlacementJobTitle}, {placementStartDate.ToString("dd/MM/yyyy")}, placement reference {PlacementRef}";
            rate.payElementCode = "PERM";
            rate.period = "FIXED";
            rate.periodDurationSpecified = true;
            rate.periodDuration = 480;
            rate.priorityOrderSpecified = true;
            rate.priorityOrder = 0;
            rate.selectableByWorkersSpecified = true;
            rate.selectableByWorkers = false;
            rate.timePattern = "default";
            rate.timesheetFields = "DAY";
            placement.rates[0] = rate;

            return placement;
        }


        private void MapConsultantSplit(Dtos.RsmInherited.Placement placement)
        {
            if (ConsultantSplits == null || ConsultantSplits.Count < 1) return;

            placement.splitCommissions = new SplitCommission[ConsultantSplits.Count];
            for (int i = 0; i < ConsultantSplits.Count; i++)
            {
                var split = new Dtos.RsmInherited.ConsultantSplit();
                split.ExternalUserId = ConsultantSplits[i].Consultant.Id;
                split.weightSpecified = true;
                split.weight = ConsultantSplits[i].Split / 100; 
                placement.splitCommissions[i] = split;
            }

        }



    }
}

