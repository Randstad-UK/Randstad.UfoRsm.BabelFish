using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Paye
    {
        public StudentLoan StudentLoan { get; set; }
        public PostgradLoan PostgradLoan { get; set; }
        public Bank Account { get; set; }
        public StarterDeclaration StartDec { get; set; }
        public string PAI { get; set; }
        public string EmailPayslips { get; set; }
        public string PaymentMethod { get; set; }
        public string PayrollRef { get; set; }
    }
}
