using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Expense
    {
        public AssignmentRate Rate { get; set; }
        public string City { get; set; }
        public string BusinessPurpose { get; set; }
        public string CopyReceipt { get; set; }
        public string RecordTypeId { get; set; }
        public string RecordTypeName { get; set; }
        public string FeeName { get; set; }
        public string Status { get; set; }
        public string PaymentType { get; set; }
        public string ExpenseCategory { get; set; }
        public string ExpenseType { get; set; }
        public decimal? Quantity { get; set; }
        public string Comments { get; set; }
        public decimal? DaysAwaitingApproval { get; set; }
        public Double? AmountWithVat { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public DateTime? ExpenseDate { get; set; }
        public DateTime? TransactionDate { get; set; }
        public Decimal? Amount { get; set; }
        public Decimal? Vat { get; set; }
        public string ExpenseRef { get; set; }
        public Client Client { get; set; }

        public bool IsMapped { get; set; }
    }
}
