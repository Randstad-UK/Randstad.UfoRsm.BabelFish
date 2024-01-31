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

       
    }
}
