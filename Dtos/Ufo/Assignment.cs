﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        //required refs
        public string AssignmentRef { get; set; }
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
        public Client Client { get; set; }
        public Client Hle { get; set; }
        public Candidate Candidate { get; set; }

        public Dtos.RsmInherited.Placement MapAssignment(Dictionary<string, string> tomCodes, ILogger logger, Dictionary<string, string> rateCodes, Guid correlationId)
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

            placement.invoiceRequiresPOSpecified = false;
            if (PoRequired != null)
            {
                placement.invoiceRequiresPOSpecified = true;
                placement.invoiceRequiresPO = PoRequired;
            }

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

            placement.customText1 = ExternalRef;
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

            placement.clientSite = Client.ClientName;

            MapRates(rateCodes, placement);

            placement.timesheetApprovalRoute = "Auto Approval Route";
            placement.chargeableExpenseApprovalRoute = "Auto Approval Route";
            placement.nonChargeableExpenseApprovalRoute = "Auto Approval Route";
            placement.expenseTemplate = "Standard Expenses";

            placement.roundToNearestMinSpecified = true;
            placement.roundToNearestMin = 1;
            MapLtdValues(placement);

            MapPayeValues(placement);


            return placement;
        }

        private void MapPayeValues(Dtos.RsmInherited.Placement placement)
        {
            if (Candidate.PayType != PaymentTypes.PAYE) return;

            if (HolidayPay.ToLower() == "rolled up holiday pay")
            {
                placement.holidayAccrualRateSpecified = true;
                placement.holidayAccrualRate = 0;

                placement.holidayAccrualRatePostAWRSpecified = true;
                placement.holidayAccrualRatePostAWR = 0;
            }

            if (HolidayPay.ToLower() == "accrue holiday pay" && (PreAwrHolidayPercentage!=null || PostAwrHolidayPercentage!=null))
            {
                if (PreAwrHolidayPercentage != null)
                {
                    placement.holidayAccrualRateSpecified = true;
                    placement.holidayAccrualRate = PreAwrHolidayPercentage;
                }

                if (PostAwrHolidayPercentage != null)
                {
                    placement.holidayAccrualRatePostAWRSpecified = true;
                    placement.holidayAccrualRatePostAWR = PostAwrHolidayPercentage;
                }
            }

            if (HolidayPay.ToLower() == "accrue holiday pay" && (PreAwrHolidayPercentage != null || PostAwrHolidayPercentage != null))
            {

                if (EnhancedHolidayDays != null)
                {
                    placement.holidayAccrualRateSpecified = true;
                    var perc = Math.Round((decimal)EnhancedHolidayDays / (260 - (decimal)EnhancedHolidayDays), 4, MidpointRounding.AwayFromZero);
                    placement.holidayAccrualRate = perc;
                }
            }

            if (AwrParityHolidaysDay1)
            {
                if (EnhancedHolidayDays != null && PostAwrHolidayPercentage==null)
                {
                    placement.holidayAccrualRateSpecified = true;
                    var perc = Math.Round((decimal)EnhancedHolidayDays / (260 - (decimal)EnhancedHolidayDays), 4, MidpointRounding.AwayFromZero);
                    placement.holidayAccrualRate = perc;

                    placement.holidayAccrualRatePostAWRSpecified = true;
                    placement.holidayAccrualRatePostAWR = perc;
                }

                if (EnhancedHolidayDays == null && PostAwrHolidayPercentage == null)
                {
                    placement.holidayAccrualRateSpecified = true;
                    placement.holidayAccrualRate = PostAwrHolidayPercentage;

                    placement.holidayAccrualRatePostAWRSpecified = true;
                    placement.holidayAccrualRatePostAWR = PostAwrHolidayPercentage;

                }
            }
        }

        private void MapLtdValues(Dtos.RsmInherited.Placement placement)
        {
            if (Candidate.PayType != PaymentTypes.LTD) return;

            placement.cisApplicable = Mappers.MapBool(Candidate.LtdCompany.Cis);
        }

        private void MapConsultantSplit(Dtos.RsmInherited.Placement placement)
        {
            if (ConsultantSplits == null) return;

            placement.splitCommissions = new SplitCommission[ConsultantSplits.Count];
            
            for (int i = 0; i < ConsultantSplits.Count; i++)
            {
                var split = new Dtos.RsmInherited.ConsultantSplit();
                split.ExternalUserId = ConsultantSplits[i].Consultant.Id;
                split.weightSpecified = true;
                split.weight = ConsultantSplits[i].Split/100;
                placement.splitCommissions[i] = split;
            }

        }

        private void MapRates(Dictionary<string, string> rateCodes, Dtos.RsmInherited.Placement placement)
        {
            if (Rates == null) return;

            var noExpenses = Rates.Where(x => x.RateType != "Expense Rate").ToList();

            if (noExpenses == null || noExpenses.Count() == 0) return;

            var rateList = new List<RSM.Rate>();

            var priorityOrder = 1;

            foreach (var rate in noExpenses)
            {
                //RSM do not hold expense rates it goes through on the expense item
                if (rate.RateType == "Expense Rate")
                {
                    continue;
                }

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

