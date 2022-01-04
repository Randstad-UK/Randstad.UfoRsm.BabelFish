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

        public string Gender { get; set; }
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
                worker.dateOfBirth = DateOfBirth;

            if (StartDate != null)
                worker.dateOfJoining = StartDate;

            worker.defaultPaymentCurrency = "GBP";
            worker.emailDisabledSpecified = true;
            worker.emailDisabled = true;
            worker.emailTimesheetRemindersSpecified = false;
            worker.excludeFromAWRSpecified = false;
            worker.externalReference = CandidateRef;
            worker.gender = Mappers.MapGender(Gender);

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

            MapPaye(worker);
            MapLtd(worker);
            MapUmbrella(worker);
            MapOutsourced(worker);

            return worker;
        }

        private void MapPaye(RSM.Worker worker)
        {
            if (PayType != PaymentTypes.PAYE) return;

            //TODO: update inputHOlidayScheme and PaymentFrequency when RMS is set up
            //worker.inpayHolidayScheme = "inputHolidayScheme";

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

            

            GetStarterDec(worker);
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

            //TODO: Update limitedCompany VAT Code once set up in RMS
            worker.limitedCompany.vatCode = "Standard";

            //map address
            worker.ltdCompanyContact = new Contact();
            worker.ltdCompanyContact.address = LtdCompany.InvoiceAddress.GetAddress();
            
            //TODO: currently don't export department for LTD not even sure we have it
            //worker.ltdCompanyContact.department = "Department";

            worker.ltdCompanyContact.email = EmailAddress;
            worker.ltdCompanyContact.externalId = EmployeeFileId;
            worker.ltdCompanyContact.firstname = Name;
            worker.ltdCompanyContact.lastname = Surname;
            worker.ltdCompanyContact.mobile = Mobile;
            worker.ltdCompanyContact.phone = Phone;

            //TODO: Update once LTD Payment frequency set up in RMS
            //worker.paymentFrequency = "weekly";


            worker.paymentMethod = PaymentMethod;


            worker.selfBillingSpecified = true;
            worker.selfBilling = false;
            if (LtdCompany.SelfBill == "Yes")
            {

                worker.selfBilling = true;
            }

            worker.utr = LtdCompany.UtrNumber;

        }

        private void MapUmbrella(RSM.Worker worker)
        {
            if (PayType != PaymentTypes.Umbrella) return;

            worker.workerType = "UMB";
            worker.limitedCompanyProviderExternalId = UmbrellaAgency.AslRef;

            //TODO: Set payment frequency for umbrella candidate once updated in RMS
            //worker.paymentFrequency = "weekly";

            worker.paymentMethod = "BACS";
        }

        private void MapOutsourced(RSM.Worker worker)
        {
            if (PayType != PaymentTypes.Outsourced) return;

            worker.workerType = "UMB";
            worker.limitedCompanyProviderExternalId = OutsourcedAgency.AslRef;

            //TODO: Set payment frequency for outsourced candidate once updated in RMS
            //worker.paymentFrequency = "weekly";

            worker.paymentMethod = "BACS";
        }


        private void GetStarterDec(RSM.Worker worker)
        {
            if (Paye.StudentLoan == null)
            {
                _logger.Warn($"Candidate {CandidateRef} has no student loan values", _correlationId, this, this.CandidateId, "Dtos.Ufo.Candidate", null);
                return;
            }

            if (Paye.PostgradLoan == null)
            {
                _logger.Warn($"Candidate {CandidateRef} has no post grad loan values", _correlationId, this, this.CandidateId, "Dtos.Ufo.Candidate", null);
                return;
            }

            if (
                Paye.StartDec.ToString()=="A" &&
                Paye.StudentLoan.HasLoan == "Yes" && 
                Paye.StudentLoan.PayingBackDirectly == "Yes" &&
                Paye.StudentLoan.LeftBeforeLast6thApril == "No" &&

                Paye.PostgradLoan.HasLoan == "No" &&
                Paye.PostgradLoan.PayingBackDirectly == "No" && 
                Paye.PostgradLoan.LeftBeforeLast6thApril == "No")
            {
                worker.starterStatementA = "AYY NNNN";
            }

            if (
                Paye.StartDec.ToString() == "B" &&
                Paye.StudentLoan.HasLoan == "No" &&

                Paye.PostgradLoan.HasLoan == "No" &&
                Paye.PostgradLoan.PayingBackDirectly == "No" &&
                Paye.PostgradLoan.LeftBeforeLast6thApril == "No")
            {
                worker.starterStatementA = "BN   NNN";
            }

            if (
                Paye.StartDec.ToString() == "C" &&
                Paye.StudentLoan.HasLoan == "Yes" &&
                Paye.StudentLoan.PayingBackDirectly == "No" &&
                Paye.StudentLoan.PlanType == "1" &&
                Paye.StudentLoan.LeftBeforeLast6thApril == "Yes" &&

                Paye.PostgradLoan.HasLoan == "No" &&
                Paye.PostgradLoan.PayingBackDirectly == "No" &&
                Paye.PostgradLoan.LeftBeforeLast6thApril == "No")
            {
                worker.starterStatementA = "CYN1YNNN";
            }

            if (
                Paye.StartDec.ToString() == "A" &&
                Paye.StudentLoan.HasLoan == "No" &&

                Paye.PostgradLoan.HasLoan == "Yes" &&
                Paye.PostgradLoan.PayingBackDirectly == "Yes" &&
                Paye.PostgradLoan.LeftBeforeLast6thApril == "No")
            {
                worker.starterStatementA = "AN   YYN";
            }

            if (
                Paye.StartDec.ToString() == "B" &&
                Paye.StudentLoan.HasLoan == "No" &&

                Paye.PostgradLoan.HasLoan == "Yes" &&
                Paye.PostgradLoan.PayingBackDirectly == "No" &&
                Paye.PostgradLoan.LeftBeforeLast6thApril == "Yes")
            {
                worker.starterStatementA = "AN   YNY";
            }
        }
    }
}
