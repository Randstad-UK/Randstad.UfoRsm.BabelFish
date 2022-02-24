using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Randstad.Logging;
using Randstad.UfoRsm.BabelFish.Helpers;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Candidate : ObjectBase
    {
        public string CandidateId { get; set; }
        public bool? LiveInPayroll { get; set; }

        public string CandidateRef { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public string Sex { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string NiNumber { get; set; }

        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string EmailAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string PayrollRefNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EmployeeFileId { get; set; }
        public CandidateStatus Status { get; set; }

        public Address HomeAddress { get; set; }

        public LtdCompany LtdCompany { get; set; }
        public ThirdPartyAgency UmbrellaAgency { get; set; }
        public ThirdPartyAgency OutsourcedAgency { get; set; }
        public Paye Paye { get; set; }
        public PaymentTypes? PayType { get; set; }


        public Team OperatingCo { get; set; }

        private ILogger _logger = null;
        private Guid _correlationId;

        public RSM.Worker MapWorker(Dictionary<string, string> tomCodes, ILogger logger, Guid correlationId)
        {
            
            _logger = logger;
            _correlationId = correlationId;

            var worker = new RSM.Worker();
            
            worker.canLoginSpecified = true;
            worker.canLogin = false;

            worker.commsDisabledSpecified = true;
            worker.commsDisabled = true;
            worker.dateOfBirthSpecified = false;

            try
            {
                worker.department = tomCodes[Unit.FinanceCode];
            }
            catch (Exception exp)
            {
                throw new Exception("Problem mapping Division for candidate");
            }

            worker.email = EmailAddress;
            worker.externalId = PayrollRefNumber;
            worker.lastname = Surname;
            worker.firstname = Name;
            worker.title = Title;

            if (DateOfBirth != null)
            {
                worker.dateOfBirth = DateOfBirth;
                worker.dateOfBirthSpecified = true;
            }

            if (StartDate != null)
            {
                worker.dateOfJoining = StartDate;
                worker.dateOfJoiningSpecified = true;
            }

            worker.defaultPaymentCurrency = "GBP";
            worker.emailDisabledSpecified = true;
            worker.emailDisabled = true;
            worker.emailTimesheetRemindersSpecified = false;
            worker.excludeFromAWRSpecified = false;
            worker.externalReference = CandidateRef;
            worker.gender = Mappers.MapGender(Sex);

            worker.isCISSpecified = false;
            worker.isInPayLinkedSpecified = false;

            worker.leaverDateSpecified = false;
            if (EndDate != null)
            {
                worker.leaverDateSpecified = true;
                worker.leaverDate = EndDate;
            }

            worker.onHoldSpecified = false;
            worker.payAsPAYESpecified = false;
            worker.starterStatementDSpecified = false;
            worker.week1Month1Specified = false;
            

            worker.emailPayslipSpecified = false;
            worker.selfBillingSpecified = false;

            worker.nationalInsuranceNumber = NiNumber;

            worker.address = HomeAddress.GetAddress();

            MapPaye(worker);
            MapLtd(worker);
            MapUmbrella(worker);
            MapOutsourced(worker);
            return worker;
        }

        private void MapPaye(RSM.Worker worker)
        {
            if (PayType != PaymentTypes.PAYE) return;

            //TODO: (DONE) update inputHOlidayScheme and PaymentFrequency when RMS is set up
            worker.inpayHolidayScheme = "HS1";

            //TODO: update payment frequency when set up in RSM
            worker.paymentFrequency = "Weekly";
            worker.workerType = "PAYE";
            
            worker.bankAccount = new RSM.BankAccount();
            worker.bankAccount.accountName = Paye.Account.AccountName;
            worker.bankAccount.accountNumber= Paye.Account.AccountNumber;
            worker.bankAccount.sortCode = Paye.Account.SortCode;
            worker.bankAccount.buildingSocRollNum = Paye.Account.BuildingSocietyRef;
            worker.bankAccount.branch = Paye.Account.BankName;

            worker.paymentMethod = PaymentMethod;


            if (!string.IsNullOrEmpty(Paye.Account.BuildingSocietyRef) && string.IsNullOrEmpty(Paye.Account.BuildingSocietyRef) && !string.IsNullOrEmpty(Paye.Account.BuildingSocietyRef))
                worker.bankAccount.buildingSocRollNum = Paye.Account.AccountName;

            worker.emailPayslipSpecified = true;
            switch (Paye.EmailPayslips.ToLower())
            {
                case "post":
                    worker.emailPayslip = false;
                    break;
                case "email":
                    worker.emailPayslip = true;
                    break;
            }

            worker.vatCode = "T0";

            GetStarterDec(worker);
            GetStudentLoan(worker);
            GetPostgradLoan(worker);
        }

        private void MapLtd(RSM.Worker worker)
        {
            if (PayType != PaymentTypes.LTD) return;

            worker.workerType = "LTD";
            worker.isCISSpecified = true;
            worker.isCIS = false;
            if (LtdCompany.Cis == "Yes")
            {
                worker.workerType = "CIS";
                worker.isCIS = true;
                worker.cisBusinessType = "Company";
                worker.cisCompanyRegNo = LtdCompany.RegNo;
                worker.cisPercentageSpecified = true;
                worker.cisPercentage = 30;
                worker.cisTradingName = LtdCompany.Name;
            }

            //TODO: not required until CPE
            //worker.customText5 = LtdCompany.PLIOptOut;

            worker.limitedCompany = new RSM.Company();
 
            worker.bankAccount = new RSM.BankAccount();
            worker.bankAccount.accountName = LtdCompany.BankAccount.AccountName;
            worker.bankAccount.accountNumber = LtdCompany.BankAccount.AccountNumber;
            worker.bankAccount.sortCode = LtdCompany.BankAccount.SortCode;
            worker.bankAccount.buildingSocRollNum = LtdCompany.BankAccount.BuildingSocietyRef;

            //map the limited company
            worker.limitedCompany = new Company();
            worker.limitedCompany.companyNo = LtdCompany.RegNo;
            worker.limitedCompany.companyVatNo = LtdCompany.VatNumber;
            worker.limitedCompany.invoiceDeliveryMethodSpecified = true;
            worker.limitedCompany.invoiceDeliveryMethod = 1;
            worker.limitedCompany.invoicePeriodSpecified = true;
            worker.limitedCompany.invoicePeriod = 1;
            worker.limitedCompany.name = LtdCompany.Name;
            
            //TODO: (Done) Update limitedCompany VAT Code once set up in RMS
            worker.limitedCompany.vatCode = "T1";
            
            worker.limitedCompany.invoicingContact =new Contact();
            worker.limitedCompany.invoicingContact.address = LtdCompany.InvoiceAddress.GetAddress();
            
            //TODO: currently don't export department for LTD not even sure we have it
            //worker.ltdCompanyContact.department = "Department";

            worker.limitedCompany.invoicingContact.email = EmailAddress;
            worker.limitedCompany.invoicingContact.externalId = EmployeeFileId;
            worker.limitedCompany.invoicingContact.firstname = Name;
            worker.limitedCompany.invoicingContact.lastname = Surname;
            worker.limitedCompany.invoicingContact.mobile = Mobile;
            worker.limitedCompany.invoicingContact.phone = Phone;

            //TODO: Update once LTD Payment frequency set up in RMS
            worker.paymentFrequency = "Weekly";


            worker.paymentMethod = PaymentMethod;


            worker.selfBillingSpecified = true;
            worker.selfBilling = false;
            if (LtdCompany.SelfBill == "Yes")
            {

                worker.selfBilling = true;
            }

            var tenDigits = new Regex(@"^\d{10}$");

            if (tenDigits.IsMatch(LtdCompany.UtrNumber))
            {
                worker.utr = LtdCompany.UtrNumber;
            }
            else
            {
                worker.utr = "0000000000";
            }

        }

        private void MapUmbrella(RSM.Worker worker)
        {
            if (PayType != PaymentTypes.Umbrella) return;

            worker.workerType = "UMB";
            worker.limitedCompanyProviderExternalId = UmbrellaAgency.AslRef;

            //TODO: Set payment frequency for umbrella candidate once updated in RMS
            worker.paymentFrequency = "Weekly";

            worker.paymentMethod = "BACS";
        }

        private void MapOutsourced(RSM.Worker worker)
        {
            if (PayType != PaymentTypes.Outsourced) return;

            worker.workerType = "UMB";
            worker.limitedCompanyProviderExternalId = OutsourcedAgency.AslRef;

            //TODO: Set payment frequency for outsourced candidate once updated in RMS
            worker.paymentFrequency = "Weekly";

            worker.paymentMethod = "BACS";
            worker.email = "outsourcedworker@randstad.co.uk";
        }


        private void GetStarterDec(RSM.Worker worker)
        {
            if (Paye.StudentLoan == null)
            {
                _logger.Warn($"Candidate {CandidateRef} has no student loan values", _correlationId, this,
                    this.CandidateId, "Dtos.Ufo.Candidate", null);
                return;
            }

            if (Paye.PostgradLoan == null)
            {
                _logger.Warn($"Candidate {CandidateRef} has no post grad loan values", _correlationId, this,
                    this.CandidateId, "Dtos.Ufo.Candidate", null);
                return;
            }

            worker.starterStatementA = Paye.StartDec.ToString();
            
        }

        private void GetStudentLoan(RSM.Worker worker)
        {
            switch (Paye.StudentLoan.HasLoan)
            {

                case "Yes":
                {
                    worker.starterStatementA = worker.starterStatementA + "Y";
                    break;
                }
                case "No":
                {
                    worker.starterStatementA = worker.starterStatementA + "N   ";
                    return;
                }
            }

            switch (Paye.StudentLoan.PayingBackDirectly)
            {
                case "Yes":
                {
                    worker.starterStatementA = worker.starterStatementA + "Y";

                    //no plan type
                    worker.starterStatementA = worker.starterStatementA + " ";
                    break;
                }
                case "No":
                {
                    worker.starterStatementA = worker.starterStatementA + "N";
                    worker.starterStatementA = worker.starterStatementA + Paye.StudentLoan.PlanType;
                    break;
                }
                default:
                {
                    worker.starterStatementA = worker.starterStatementA + "Y";

                    //no plan type
                    worker.starterStatementA = worker.starterStatementA + " ";
                    break;
                }
            }

            switch (Paye.StudentLoan.LeftBeforeLast6thApril)
            {
                case "Yes":
                {
                    worker.starterStatementA = worker.starterStatementA + "Y";
                    break;
                }
                case "No":
                {
                    worker.starterStatementA = worker.starterStatementA + "N";
                    break;
                }
            }

        }

        private void GetPostgradLoan(RSM.Worker worker)
        {
            switch (Paye.PostgradLoan.HasLoan)
            {
                case "Yes":
                {
                    worker.starterStatementA = worker.starterStatementA + "Y";
                    break;
                }
                default:
                {
                    worker.starterStatementA = worker.starterStatementA + "N";
                    break;
                }
            }


            switch (Paye.PostgradLoan.PayingBackDirectly)
            {
                case "Yes":
                {
                    worker.starterStatementA = worker.starterStatementA + "Y";
                    break;
                }
                case "No":
                {
                    worker.starterStatementA = worker.starterStatementA + "N";
                    break;
                }
                default:
                {
                    worker.starterStatementA = worker.starterStatementA + "N";
                    break;
                }
            }

            switch (Paye.PostgradLoan.LeftBeforeLast6thApril)
            {
                case "Yes":
                {
                    worker.starterStatementA = worker.starterStatementA + "Y";
                    break;
                }
                case "No":
                {
                    worker.starterStatementA = worker.starterStatementA + "N";
                    break;
                }
                default:
                {
                    worker.starterStatementA = worker.starterStatementA + "N";
                    break;
                }

            }

        }
        
    }
}
