using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using Randstad.UfoRsm.BabelFish.Helpers;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class LtdCompany : ObjectBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Cis { get; set; }
        public Address InvoiceAddress { get; set; }
        public string RegNo { get; set; }
        public string SelfBill { get; set; }
        public string UtrNumber { get; set; }
        public string VatNumber { get; set; }
        public Bank BankAccount { get; set; }
        public string PLIOptOut { get; set; }
        public List<Candidate> Candidates { get; set; }

        //public Company GetCompany()
        //{
        //    var company = new Company();
        //    company.companyNo = RegNo;
        //    company.companyVatNo = VatNumber;
        //    company.externalId = 
        //}

        /*
        public Supplier GetSupplier(string paymentMethod)
        {
            var supplier = new Supplier();

            if(InvoiceAddress!=null)
                supplier.SupplierInvoiceAddress = InvoiceAddress.GetAddress();

            supplier.BusinessName = Name;
            supplier.VATNumber = VatNumber;
            supplier.SupplierType = SupplierType.C;
            supplier.CompanyRegistrationNo = RegNo;
            supplier.EntityReference = supplier.SupplierRef;

            supplier.SupplierBankAccount = new BankAccount();
            supplier.SupplierBankAccount.SortCode = BankAccount.SortCode;
            supplier.SupplierBankAccount.AccountName = BankAccount.AccountName;
            supplier.SupplierBankAccount.AccountNumber = BankAccount.AccountNumber;
            supplier.SupplierBankAccount.SocietyName = BankAccount.SocietyName;
            supplier.SupplierBankAccount.SocietyNum = BankAccount.SocietyNum;
            supplier.SupplierBankAccount.PayMethod = MapPaymentMethod(paymentMethod);

            if(!string.IsNullOrEmpty(BankAccount.SocietyNum) && string.IsNullOrEmpty(BankAccount.SocietyName) && !string.IsNullOrEmpty(BankAccount.AccountName))
                supplier.SupplierBankAccount.SocietyName = BankAccount.AccountName;


            supplier.SelfBillingEnabled = Mappers.MapYesNo(SelfBill);
            supplier.UTRRef = UtrNumber;
            supplier.SupplierRef = Name;
            supplier.IsStartChecked = true;
            
            MapLegalStatus(supplier);


            return supplier;

        }

        private void MapLegalStatus(Sti.Supplier supplier)
        {
            supplier.LegalStatus = SupplierLegalStatus.L;
            supplier.CISLegalStatus = null;
            if (Cis == "No") return;
            
            supplier.LegalStatus = SupplierLegalStatus.C;
            supplier.CISLegalStatus = SupplierCISLegalStatus.L;

        }

        private PayMethod MapPaymentMethod(string paymentMethod)
        {
            switch (paymentMethod.ToLower())
            {
                case "bacs":
                    return PayMethod.BACS;
                case "cheque":
                    return PayMethod.Cheque;
                default:
                    throw new Exception("Unknown Payment Method");
            }
        }
        */
    }
}
