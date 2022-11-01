using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Randstad.Logging;
using Randstad.Logging.Core;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish.Dtos.RsmInherited;
using Randstad.UfoRsm.BabelFish.Helpers;
using Randstad.UfoRsm.BabelFish.Template.Extensions;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Assignment : ObjectBase
    {
        public Team OpCo { get; set; }

        //required refs
        public string AssignmentRef { get; set; }
        public string PositionName { get; set; }
        public string ExternalRef { get; set; }

        //properties
        public string CheckIn { get; set; }
        public bool IR35 { get; set; }
        public string PoNumber { get; set; }
        public bool? PoRequired { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string AssignmentJobTitle { get; set; }
        public Consultant Owner { get; set; }
        public InvoiceAddress InvoiceAddress { get; set; }
        public Address WorkAddress { get; set; }
        public ClientContact InvoicePerson { get; set; }
        public List<AssignmentRate> Rates { get; set; }
        public List<ConsultantSplit> ConsultantSplits { get; set; }
        public string PreferredPeriod { get; set; }
        public decimal? EnhancedHolidayDays { get; set; }
        public string HolidayPay { get; set; }
        public decimal? PreAwrHolidayPercentage { get; set; }
        public decimal? PostAwrHolidayPercentage { get; set; }
        public bool AwrParityHolidaysDay1 { get; set; }
        public string CostCentre { get; set; }
        public Client Client { get; set; }
        public Client Hle { get; set; }
        public Candidate Candidate { get; set; }
        public ClientContact ClientContact { get; set; }
        public string SendRatesFormat { get; set; }

        public Dtos.RsmInherited.Placement MapAssignment(ILogger logger, Dictionary<string, string> rateCodes, Guid correlationId, List<DivisionCode> divisionCodes)
        {

            var placement = new Dtos.RsmInherited.Placement();
            placement.PAYEDeductionsOnLtdSpecified = true;
            placement.PAYEDeductionsOnLtd = IR35;
            placement.holidayAccrualRatePostAWRSpecified = false;
            placement.holidayAccrualRateSpecified = false;

            placement.agencyOnlySpecified = true;
            placement.agencyOnly = true;

            placement.bulkEntrySpecified = false;

            //TODO: Assignment CIS needs to be pulled once UFO solution specced
            placement.cisApplicableSpecified = true;
            placement.cisApplicable = false;



            placement.client = Client.MapClient(divisionCodes);
            placement.consultant = Owner.MapConsultant();

            placement.contractedHoursSpecified = true;
            placement.contractedHours = 40;

            

            if (!string.IsNullOrEmpty(EndDate))
            {
                placement.endSpecified = true;
                placement.end = DateTime.Parse(EndDate);

            }

            placement.expenseEmailApprovalSpecified = true;
            placement.expenseEmailApproval = false;

            placement.externalId = AssignmentRef;

            if (CostCentre == null)
            {
                CostCentre = "";
            }
            placement.customText4 = CostCentre;

            placement.faxbackEnabledSpecified = true;
            placement.faxbackEnabled = false;

            placement.invoiceRequiresPOSpecified = false;
            
            placement.invoiceContactOverride = InvoicePerson.MapContact();
            
            //billing requested no name
            placement.invoiceContactOverride.firstname = string.Empty;
            placement.invoiceContactOverride.lastname = string.Empty;

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

            placement.invoiceContactOverride.email = string.Empty;
            foreach (var email in invoiceEmailList)
            {
                placement.invoiceContactOverride.email = placement.invoiceContactOverride.email + email + "; ";
            }

            if (!string.IsNullOrEmpty(placement.invoiceContactOverride.email) && placement.invoiceContactOverride.email.EndsWith("; "))
            {
                placement.invoiceContactOverride.email = placement.invoiceContactOverride.email.Remove(placement.invoiceContactOverride.email.LastIndexOf(";"));
            }



            placement.invoiceContactOverride.address = InvoiceAddress.MapAddress();

            placement.jobTitle = string.IsNullOrEmpty(PositionName) ? "Not Stated" : PositionName;

            placement.jobDescription = AssignmentJobTitle;

            placement.noCommunications = "WMCL";
            placement.permSpecified = true;
            placement.perm = false;

            placement.purchaseBranch = Unit.Name;
            placement.purchaseCostCentre = Unit.FinanceCode;

            //ToDo: remove once fix confirmed
            //placement.purchaseDivision = OpCo.Name;

            placement.purchaseDivision = divisionCodes.SingleOrDefault(x=>x.Code==Unit.FinanceCode)?.Division;

            if (!string.IsNullOrEmpty(PoNumber))
            {
                placement.purchaseOrderNum = PoNumber.Trim();
            }

            placement.roundToNearestMinSpecified = true;
            placement.roundToNearestMin = 1;

            placement.salesBranch = placement.purchaseBranch;
            placement.salesCostCentre = placement.purchaseCostCentre;
            placement.salesDivision = placement.purchaseDivision;

            placement.siteAddress = WorkAddress.GetAddress();

            MapConsultantSplit(placement);

            

            if (!string.IsNullOrEmpty(StartDate))
            {
                placement.startSpecified = true;
                placement.start = DateTime.Parse(StartDate);
            }

            placement.timesheetEmailApprovalSpecified = true;
            placement.timesheetEmailApproval = false;
            placement.worker = Candidate.MapWorker(divisionCodes, logger, correlationId);

            placement.awrWeekSpecified = false;
            placement.excludeFromMissingTimeSpecified = true;
            placement.excludeFromMissingTime = true;

            placement.customText1 = ExternalRef;
            placement.customText2 = Hle.ClientRef;

            if (ClientContact != null)
            {
                placement.manager = ClientContact.MapContactManager(Client);
            }

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

            placement.customText5 = Candidate.Name + " " + Candidate.Surname;

            placement.clientSite = Client.ClientName;
            //max length in RSM is 90 characters
            if (placement.clientSite.Length > 80)
            {
                placement.clientSite = placement.clientSite.Substring(0, 79);
            }

            MapRates(rateCodes, placement);

            placement.timesheetApprovalRoute = "Auto Approval Route";
            placement.chargeableExpenseApprovalRoute = "Auto Approval Route";
            placement.nonChargeableExpenseApprovalRoute = "Auto Approval Route";
            placement.expenseTemplate = "Standard Expenses";

            placement.roundToNearestMinSpecified = true;
            placement.roundToNearestMin = 1;
            MapPayeValues(placement);


            return placement;
        }

        private void MapPayeValues(Dtos.RsmInherited.Placement placement)
        {
            if (Candidate.PayType != PaymentTypes.PAYE) return;



            if (!string.IsNullOrEmpty(HolidayPay) && HolidayPay.ToLower() == "rolled up holiday pay")
            {
                placement.holidayAccrualRateSpecified = true;
                placement.holidayAccrualRate = 0;

                placement.holidayAccrualRatePostAWRSpecified = true;
                placement.holidayAccrualRatePostAWR = 0;

                return;
            }

            if (AwrParityHolidaysDay1)
            {
                if (EnhancedHolidayDays != null)
                {
                    placement.holidayAccrualRateSpecified = true;
                    var perc = Math.Round((decimal)EnhancedHolidayDays / (260 - (decimal)EnhancedHolidayDays), 4, MidpointRounding.AwayFromZero);
                    placement.holidayAccrualRate = perc;

                    placement.holidayAccrualRatePostAWRSpecified = true;
                    placement.holidayAccrualRatePostAWR = perc;
                    return;
                }

                if (PostAwrHolidayPercentage != null)
                {
                    placement.holidayAccrualRateSpecified = true;
                    placement.holidayAccrualRate = PostAwrHolidayPercentage / 100;

                    placement.holidayAccrualRatePostAWRSpecified = true;
                    placement.holidayAccrualRatePostAWR = PostAwrHolidayPercentage /100;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(HolidayPay) && HolidayPay.ToLower() == "accrue holiday pay")
            {
                if (PreAwrHolidayPercentage != null)
                {
                    placement.holidayAccrualRateSpecified = true;
                    placement.holidayAccrualRate = PreAwrHolidayPercentage/100;
                }

                if (PostAwrHolidayPercentage != null)
                {
                    placement.holidayAccrualRatePostAWRSpecified = true;
                    placement.holidayAccrualRatePostAWR = PostAwrHolidayPercentage/100;
                }

                if (EnhancedHolidayDays != null)
                {
                    placement.holidayAccrualRatePostAWRSpecified = true;
                    var perc = Math.Round((decimal)EnhancedHolidayDays / (260 - (decimal)EnhancedHolidayDays), 4, MidpointRounding.AwayFromZero);
                    placement.holidayAccrualRatePostAWR = perc;
                }
            }
        }


        private void MapConsultantSplit(Dtos.RsmInherited.Placement placement)
        {
            if (ConsultantSplits == null) return;

            placement.splitCommissions = new SplitCommission[ConsultantSplits.Count];
            
            for (int i = 0; i < ConsultantSplits.Count; i++)
            {
                var split = new Dtos.RsmInherited.ConsultantSplit();
                split.ExternalUserId = "UFO"+ConsultantSplits[i].Consultant.EmployeeRef;
                split.weightSpecified = true;
                split.weight = ConsultantSplits[i].Split/100;
                placement.splitCommissions[i] = split;
            }

        }

        private void MapRates(Dictionary<string, string> rateCodes, Dtos.RsmInherited.Placement placement)
        {
            if (Rates == null) return;

            var noExpenses = Rates.Where(x => x.RateType != "Expense Rate" || x.FeeName=="Bonus" || x.FeeName== "Back Pay - Non WTR" || x.FeeName== "Back Pay - WTR").ToList();

            if (noExpenses == null || noExpenses.Count() == 0) return;

            var rateList = new List<RSM.Rate>();

            var priorityOrder = 1;

            foreach (var rate in noExpenses)
            {
                RSM.Rate postRate = null;
                var rsmRate = rate.MapRate(rateCodes, this, out postRate);

                rsmRate.priorityOrderSpecified = true;
                rsmRate.priorityOrder = priorityOrder;


                if (rate.PayUnit == "Hourly")
                {
                    rsmRate.priorityOrder = 0;
                }
                
                priorityOrder++;
                rateList.Add(rsmRate);
                if (postRate != null)
                {
                    rateList.Add(postRate);
                }
            }

            placement.rates = rateList.ToArray();
        }
    }
}

