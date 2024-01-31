using System;
using System.Collections.Generic;
using System.Text;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class SelfEmployed
    {
        public BankAccount Account { get; set; }
        public string EmailPayslips { get; set; }
        public string PaymentMethod { get; set; }
        public string PayrollRef { get; set; }
        public DateTime PLIEffectiveDate { get; set; }
        public DateTime PLIExpiryDate { get; set; }
        public string PLIOptOut { get; private set; }
        public DateTime PIIEffectiveDate { get; set; }
        public DateTime PIIExpiryDate { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNo { get; set; }
        public string SelfBill { get; set; }
        public string VatNo { get; set; }
        public string UtrNo { get; set; }
    }
}