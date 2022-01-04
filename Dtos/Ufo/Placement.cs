using System;
using System.Collections.Generic;
using System.Text;
using Randstad.UfoSti.BabelFish.Dtos.Sti;
using Randstad.UfoSti.BabelFish.Dtos.Ufo;
using Randstad.UfoSti.BabelFish.Helpers;

namespace Randstad.UfoSti.BabelFish.Dtos.Ufo
{
    public class Placement : ObjectBase
    {
        public Team OpCo { get; set; }

        public string PlacementId { get; set; }
        public string MigratedPlacementId { get; set; }
        public string PlacementRef { get; set; }
        public string PoNumber { get; set; }
        public bool? PoRequired { get; set; }
        public DateTime? StartDate { get; set; }
        public string PlacementJobTitle { get; set; }
        public string Description { get; set; }
        public string CheckIn { get; set; }
        public string CostCentre { get; set; }
        public string Contact { get; set; }
        public string Candidate { get; set; }

        public decimal Salary { get; set; }
        public decimal Fee { get; set; }
        public string CandidateRef { get; set; }
        public string ClientRef { get; set; }
        public string ClientName { get; set; }
        public string ClientId { get; set; }
        public string HleRef { get; set; }

        public Owner Owner { get; set; }
        public InvoiceAddress InvoiceAddress { get; set; }
        public string InvoiceAddressId { get; set; }

        public List<ConsultantSplit> ConsultantSplits { get; set; }

        public Sti.Placement MapPlacement(string consultantPrefixCode, Dictionary<string, string> tomCodes, Dictionary<string, string> employerRefs, out ClientAddress invoiceAddress)
        {
            var placement = new Sti.Placement();
            invoiceAddress = null;

            try
            {
                placement.EmployerRef = employerRefs[OpCo.FinanceCode];
            }
            catch(Exception exp)
            {
                throw new Exception("Problem mapping Employer ref for placement", exp);
            }

            try
            {
                placement.Division = tomCodes[Unit.FinanceCode];
            }
            catch(Exception exp)
            {
                throw new Exception("Problem mapping Division for placement", exp);
            }

            placement.Department = Unit.FinanceCode;
            placement.OpCo = Mappers.MapOpCo(OpCo.FinanceCode);
            placement.EntityReference = PlacementRef;
            placement.PlacementRef = PlacementRef;
            placement.ClientRef = ClientRef;
            placement.ClientContactName = Contact;
            placement.ConsultantCode = consultantPrefixCode + Owner.EmployeeRef;
            placement.ApplicantName = Candidate;
            placement.StartDate = StartDate;
            placement.JobTitle = PlacementJobTitle;
            placement.PONumber = PoNumber;
            placement.Salary = Salary;
            placement.PlacementValue = Fee;
            placement.CostCentre = CostCentre;
            
            
            if(CheckIn=="Checked In")
                placement.IsStartChecked = true;

            MapConsultantSplit(placement, consultantPrefixCode, tomCodes);

            if (!string.IsNullOrEmpty(InvoiceAddressId))
                placement.InvoiceAddressNumber = int.Parse(InvoiceAddressId);

            //get the invoice address
            if (InvoiceAddress != null)
            {
                invoiceAddress = InvoiceAddress.MapSingleClientAddress(placement.ClientRef, ClientName, placement.Department);
            }

            return placement;
        }

        private void MapConsultantSplit(Sti.Placement placement, string consultantPrefixCode, Dictionary<string, string> tomCodes)
        {
            placement.ConsultantSplit1 = ConsultantSplits[0].Split;
            placement.SplitDepartment1 = ConsultantSplits[0].Consultant.Unit.FinanceCode;
            placement.SplitDivision1 = tomCodes[ConsultantSplits[0].Consultant.Unit.FinanceCode];

            if (ConsultantSplits.Count != 2) return;

            placement.ConsultantCode2 = consultantPrefixCode + ConsultantSplits[1].Consultant.EmployeeRef;
            placement.ConsultantSplit2 = ConsultantSplits[1].Split;
            placement.SplitDepartment2 = ConsultantSplits[1].Consultant.Unit.FinanceCode;
            placement.SplitDivision2 = tomCodes[ConsultantSplits[0].Consultant.Unit.FinanceCode];
        }
    }
}
