using Randstad.Logging;
using RSM;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public string StudentFirstname { get; set; }
        public string StudentLastname { get; set; }
        public DateTime StudentDob { get; set; }
        public string StudentCrn { get; set; }
        public Client FundingBody { get; set; }
        public bool MultiStudentSupport { get; set; }

        private List<string> GetInvoiceContactOverrideEmails(Client client)
        {
            var invoiceEmailList = new List<string>();

            if (!string.IsNullOrEmpty(client.InvoiceEmail))
            {
                invoiceEmailList.Add(client.InvoiceEmail);
            }

            if (!string.IsNullOrEmpty(client.InvoiceEmail2))
            {
                invoiceEmailList.Add(client.InvoiceEmail2);
            }

            if (!string.IsNullOrEmpty(client.InvoiceEmail3))
            {
                invoiceEmailList.Add(client.InvoiceEmail3);
            }

            //Most of the business uses client ref for both client ref and invoice to client
            if (Division.Name == "Tuition Services" || Division.Name == "Student Support")
            {
                invoiceEmailList = new List<string>();

                if (!string.IsNullOrEmpty(FundingBody.InvoiceEmail))
                {
                    invoiceEmailList.Add(FundingBody.InvoiceEmail);
                }

                if (!string.IsNullOrEmpty(FundingBody.InvoiceEmail2))
                {
                    invoiceEmailList.Add(FundingBody.InvoiceEmail2);
                }

                if (!string.IsNullOrEmpty(FundingBody.InvoiceEmail3))
                {
                    invoiceEmailList.Add(FundingBody.InvoiceEmail3);
                }
            }

            return invoiceEmailList;
        }

        private void SetBillingControlled(RSM.Placement placement, Client client)
        {
            if (client.InvoiceDeliveryMethod != "Billing Controlled") return;

            if (placement.invoiceContactOverride == null)
            {
                placement.invoiceContactOverride = new Contact();
            }

            if (InvoiceAddress != null)
            {
                placement.invoiceContactOverride.address = InvoiceAddress.MapAddress();
            }


            var invoiceEmailList = GetInvoiceContactOverrideEmails(client);
            
            if (!invoiceEmailList.Any()) return;

            foreach (var email in invoiceEmailList)
            {
                if (placement.invoiceContactOverride == null)
                {
                    placement.invoiceContactOverride = new Contact();
                }

                placement.invoiceContactOverride.email = placement.invoiceContactOverride.email + email + "; ";
            }

            //clear last semi colon if invoice email set
            if (placement.invoiceContactOverride != null && !string.IsNullOrEmpty(placement.invoiceContactOverride.email) && placement.invoiceContactOverride.email.EndsWith("; "))
            {
                placement.invoiceContactOverride.email = placement.invoiceContactOverride.email.Remove(placement.invoiceContactOverride.email.LastIndexOf(";"));
            }

            placement.invoiceContactOverride.address = InvoiceAddress.MapAddress();

        }

        private void SetElectronic(RSM.Placement placement, Client client)
        {
            if (client.InvoiceDeliveryMethod != "Electronic") return;

            if (placement.invoiceContactOverride == null)
            {
                placement.invoiceContactOverride = new Contact();
            }

            if (InvoiceAddress != null)
            {
                placement.invoiceContactOverride.address = InvoiceAddress.MapAddress();
            }

            if (client.CentralInvoiceing == "Yes")
            {
                var invoiceEmailList = GetInvoiceContactOverrideEmails(client);

                if (!invoiceEmailList.Any()) return;

                foreach (var email in invoiceEmailList)
                {
                    placement.invoiceContactOverride.email = placement.invoiceContactOverride.email + email + "; ";
                }

                //clear last semi colon if invoice email set
                if (placement.invoiceContactOverride != null && !string.IsNullOrEmpty(placement.invoiceContactOverride.email) && placement.invoiceContactOverride.email.EndsWith("; "))
                {
                    placement.invoiceContactOverride.email = placement.invoiceContactOverride.email.Remove(placement.invoiceContactOverride.email.LastIndexOf(";"));
                }

                
                return;
            }

            if (InvoicePerson != null)
            {
                placement.invoiceContactOverride.email = InvoicePerson.EmailAddress;
            }

        }

        private void SetSelfBill(RSM.Placement placement, Client client)
        {
            if (client.InvoiceDeliveryMethod != "Self Bill") return;

            if (placement.invoiceContactOverride == null)
            {
                placement.invoiceContactOverride = new Contact();
            }

            placement.invoiceContactOverride.address = new RSM.Address()
            {
                line1 = "450 Capability Green",
                town = "Luton",
                postcode = "LU1 3LU"
            };
        }

        private void SetPost(RSM.Placement placement, Client client)
        {
            if (client.InvoiceDeliveryMethod != "Paper") return;

            if (placement.invoiceContactOverride == null)
            {
                placement.invoiceContactOverride = new Contact();
            }

            placement.invoiceContactOverride.address = InvoiceAddress.MapAddress();
        }

        private void SetFromClient(RSM.Placement placement)
        {
            //if the invoice delivery method is not set on the client then it needs to be mapped from the HLE
            if (string.IsNullOrEmpty(Client.InvoiceDeliveryMethod)) return;

            SetBillingControlled(placement, Client);
            SetElectronic(placement, Client);
            SetSelfBill(placement, Client);
            SetPost(placement, Client);
        }

        private void SetFromHle(RSM.Placement placement)
        {
            //should not map from HLE if invoice delivery is set on the client
            if (!string.IsNullOrEmpty(Client.InvoiceDeliveryMethod)) return;

            //invoice delivery method should never be blank on the HLE but just incase exit so not getting null pointer exception
            if (string.IsNullOrEmpty(Hle.InvoiceDeliveryMethod)) return;

            SetBillingControlled(placement, Hle);
            SetElectronic(placement, Hle);
            SetSelfBill(placement, Hle);
            SetPost(placement, Hle);
        }

        public Dtos.RsmInherited.Placement MapAssignment(ILogger logger, Dictionary<string, string> rateCodes, Guid correlationId, List<DivisionCode> divisionCodes)
        {

            var placement = new Dtos.RsmInherited.Placement();
            placement.PAYEDeductionsOnLtdSpecified = true;
            placement.PAYEDeductionsOnLtd = false;

            placement.holidayAccrualRatePostAWRSpecified = false;
            placement.holidayAccrualRateSpecified = false;

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
            placement.salesDepartment = CostCentre;

            placement.faxbackEnabledSpecified = true;
            placement.faxbackEnabled = false;

            placement.invoiceRequiresPOSpecified = false;

            placement.jobTitle = string.IsNullOrEmpty(PositionName) ? "Not Stated" : PositionName;

            placement.jobDescription = AssignmentJobTitle;

            placement.noCommunications = "WMCL";
            placement.permSpecified = true;
            placement.perm = false;

            placement.purchaseBranch = Unit.Name;
            placement.purchaseCostCentre = Unit.FinanceCode;


            var division = divisionCodes.SingleOrDefault(x => x.Code == Unit.FinanceCode);

            placement.purchaseDivision = division.Division;
            if (!string.IsNullOrEmpty(division.TimeSheetDuration))
            {
                placement.timesheetDateCalculatorName = division.TimeSheetDuration;
            }

            if (!string.IsNullOrEmpty(PoNumber))
            {
                placement.purchaseOrderNum = PoNumber.Trim();
            }
            else
            {
                placement.purchaseOrderNum = "";
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

            MapRates(rateCodes, placement);

            placement.timesheetApprovalRoute = "Auto Approval Route";
            placement.chargeableExpenseApprovalRoute = "Auto Approval Route";
            placement.nonChargeableExpenseApprovalRoute = "Auto Approval Route";
            placement.expenseTemplate = "Standard Expenses";

            placement.roundToNearestMinSpecified = true;
            placement.roundToNearestMin = 1;
            MapPayeValues(placement);
            MapLtdValues(placement);

            //Most of the business uses client ref for both client ref and invoice to client
            if (Division.Name == "Tuition Services" || Division.Name == "Student Support")
            {

                placement.client = FundingBody.MapClient(divisionCodes);
                placement.customText2 = FundingBody.ClientRef;
                placement.customText4 = Client.ClientName;
                placement.customText5 = StudentFirstname + " | " + StudentLastname + " | " + StudentDob.ToString("dd/MM/yyyy") + " | " + StudentCrn;
                placement.clientSite = FundingBody.ClientName;

                if (Unit.Name == "NTP Tuition Pillar")
                {
                    placement.client = Client.MapClient(divisionCodes);
                    placement.clientSite = Client.ClientName;
                    placement.customText2 = Client.ClientRef;
                }

            }

            //max length in RSM is 90 characters
            if (placement.clientSite.Length > 80)
            {
                placement.clientSite = placement.clientSite.Substring(0, 79);
            }

            SetFromClient(placement);
            SetFromHle(placement);

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

            //FO-5986 if the override fields have values and the holiday pay is not
            if (!string.IsNullOrEmpty(HolidayPay) 
                && HolidayPay.ToLower() != "rolled up holiday pay"
                && PreAwrHolidayPercentage!=null
                && PostAwrHolidayPercentage!=null)
            {
                placement.holidayAccrualRateSpecified = true;
                placement.holidayAccrualRate = PreAwrHolidayPercentage / 100;

                placement.holidayAccrualRatePostAWRSpecified = true;
                placement.holidayAccrualRatePostAWR = PostAwrHolidayPercentage / 100;
            }

            //this is no longer required
            /*if (AwrParityHolidaysDay1)
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
                    placement.holidayAccrualRatePostAWR = PostAwrHolidayPercentage / 100;
                    return;
                }
            }

            if (HolidayPay.ToLower() == "accrue holiday pay")
            {
                if (PreAwrHolidayPercentage != null)
                {
                    placement.holidayAccrualRateSpecified = true;
                    placement.holidayAccrualRate = PreAwrHolidayPercentage / 100;
                }

                if (PostAwrHolidayPercentage != null)
                {
                    placement.holidayAccrualRatePostAWRSpecified = true;
                    placement.holidayAccrualRatePostAWR = PostAwrHolidayPercentage / 100;
                }

                if (EnhancedHolidayDays != null)
                {
                    placement.holidayAccrualRatePostAWRSpecified = true;
                    var perc = Math.Round((decimal)EnhancedHolidayDays / (260 - (decimal)EnhancedHolidayDays), 4, MidpointRounding.AwayFromZero);
                    placement.holidayAccrualRatePostAWR = perc;
                }
            }*/
        }

        private void MapLtdValues(Dtos.RsmInherited.Placement placement)
        {
            if (Candidate.PayType != PaymentTypes.LTD) return;

            placement.PAYEDeductionsOnLtdSpecified = true;
            placement.PAYEDeductionsOnLtd = IR35;
        }


        private void MapConsultantSplit(Dtos.RsmInherited.Placement placement)
        {
            if (ConsultantSplits == null) return;

            placement.splitCommissions = new SplitCommission[ConsultantSplits.Count];

            for (int i = 0; i < ConsultantSplits.Count; i++)
            {
                var split = new Dtos.RsmInherited.ConsultantSplit();
                split.ExternalUserId = "UFO" + ConsultantSplits[i].Consultant.EmployeeRef;
                split.weightSpecified = true;
                split.weight = ConsultantSplits[i].Split / 100;
                placement.splitCommissions[i] = split;
            }

        }

        private void MapRates(Dictionary<string, string> rateCodes, Dtos.RsmInherited.Placement placement)
        {
            if (Rates == null) return;

            var noExpenses = Rates.Where(x => x.RateType != "Expense Rate" || x.FeeName == "Bonus" || x.FeeName == "Back Pay - Non WTR" || x.FeeName == "Back Pay - WTR").ToList();

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

