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
        public bool PoRequired { get; set; }
        public DateTime StartDate { get; set; }
        public string PlacementJobTitle { get; set; }

        public string CandidateRef { get; set; }
        public string CandidateEmail { get; set; }
        public string CandidateFirstName { get; set; }
        public string CandidateLastName { get; set; }
        public string CandidatePayrollRef { get; set; }
        public string CandidatePaymentType { get; set; }
        public decimal Salary { get; set; }
        public decimal Fee { get; set; }
        public string CheckIn { get; set; }

        public Owner Owner { get; set; }
        public InvoiceAddress InvoiceAddress { get; set; }


        public Client Client { get; set; }

        public List<ConsultantSplit> ConsultantSplits { get; set; }

        public Dtos.RsmInherited.Placement MapPlacement(Dictionary<string, string> tomCodes, ILogger logger, Guid correlationId)
        {

            var placement = new Dtos.RsmInherited.Placement();
            placement.PAYEDeductionsOnLtdSpecified = true;
            placement.agencyOnlySpecified = true;
            placement.agencyOnly = true;
            placement.bulkEntrySpecified = false;

            placement.cisApplicableSpecified = true;
            placement.cisApplicable = false;

            placement.client = Client.MapClient();
            placement.consultant = Owner.MapConsultant();

            placement.contractedHoursSpecified = true;
            placement.contractedHours = 0;

            placement.endSpecified = true;
            placement.end = StartDate;

            placement.expenseEmailApprovalSpecified = true;
            placement.expenseEmailApproval = false;

            placement.externalId = PlacementRef;

            placement.faxbackEnabledSpecified = true;
            placement.faxbackEnabled = false;

            placement.holidayAccrualRateSpecified = false;

            placement.invoiceRequiresPOSpecified = false;
            if (PoRequired != null)
            {
                placement.invoiceRequiresPOSpecified = true;
                placement.invoiceRequiresPO = PoRequired;
            }

            placement.invoiceContactOverride = InvoiceAddress.MapContact();
            placement.jobDescription = PlacementJobTitle;

            //todo: Assignment manager needs set to default manager set up in RSM
            placement.manager = new Manager();

            placement.noCommunications = "WMCL";
            placement.permSpecified = true;
            placement.perm = true;

            placement.purchaseBranch = Unit.Name;
            placement.purchaseCostCentre = tomCodes[Unit.FinanceCode];
            placement.purchaseDivision = OpCo.Name;
            placement.purchaseOrderNum = PoNumber;

            placement.roundToNearestMinSpecified = true;
            placement.roundToNearestMin = 1;

            placement.salesBranch = Unit.Name;
            placement.salesCostCentre = tomCodes[Unit.FinanceCode];

            placement.siteAddress = Client.WorkAddress.GetAddress();

            MapConsultantSplit(placement);

            placement.start = StartDate;

            placement.timesheetEmailApprovalSpecified = true;
            placement.timesheetEmailApproval = false;
            placement.worker = new Worker();
            placement.worker.externalReference = CandidateRef;
            placement.worker.externalId = CandidatePayrollRef;
            placement.worker.workerType = CandidatePaymentType;
            placement.worker.lastname = CandidateFirstName;
            placement.worker.firstname = CandidateLastName;
            placement.worker.email = CandidateEmail;

            //TODO: Placement mapping set payment freqency when supplied by finance
            placement.worker.paymentFrequency = "Weekly";

            placement.awrWeekSpecified = false;
            placement.excludeFromMissingTimeSpecified = true;
            placement.excludeFromMissingTime = false;

            Rate r = new Rate();
            r.charge = Fee;
            r.pay = 0;
            placement.rates = new Rate[1];
            placement.rates[0] = r;
            AddDefaultManager(placement);
            return placement;
        }

        private void AddDefaultManager(RSM.Placement placement)
        {
            placement.manager = new Manager();
            placement.manager.externalId = "MAN001";
            placement.manager.clientExternalId = "CLI_001";
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

    }
}

