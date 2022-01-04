using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish.Helpers;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Client : ObjectBase
    {
        public string ClientId { get; set; }
        public string ClientRef { get; set; }
        public string MigratedClientId { get; set; }
        public string ClientName { get; set; }
        public bool? IR35 { get; set; }

        public string Status { get; set; }
        public string VatNo { get; set; }
        public bool? NoVat { get; set; }
        public string PoRequired { get; set; }
        public string InvoiceDeliveryMethod { get; set; }
        public string InvoiceEmail { get; set; }
        public string InvoiceEmail2 { get; set; }
        public string InvoiceEmail3 { get; set; }
        public string InvoiceAddressNo { get; set; }

        public Client HleClient { get; set; }
        public Client ParentClient { get; set; }
        public bool? IsCheckedIn { get; set; }
        public bool? ActiveInSirenum { get; set; }
        //public Owner Owner { get; set; }
        public bool? IsLegalHirer { get; set; }
        public Address WorkAddress { get; set; }
        public Address WorkAddressFromInvoiceAddress { get; set; }
       // public List<InvoiceAddress> InvoiceAddresses { get; set; }

        public Team OpCo { get; set; }


        public RSM.Client MapClient()
        {
            var client = new RSM.Client();

            client.awrCanConfirmComparableSpecified = true;
            client.awrCanConfirmComparable = false;

            client.awrComparatorEntryByAgencySpecified = true;
            client.awrComparatorEntryByAgency = false;

            client.awrComparatorEntryByAgencySpecified = true;
            client.awrComparatorEntryByAgency = false;

            client.awrCanConfirmComparableSpecified = true;
            client.awrCanConfirmComparable = false;

            client.awrUseAgencyDefaultsSpecified = true;
            client.awrUseAgencyDefaults = true;

            client.intermediarySpecified = true;
            client.intermediary = false;

            client.onHoldSpecified = true;
            client.onHoldSpecified = false;

            client.purchaseOrderNumberAtShiftLevelSpecified = true;
            client.purchaseOrderRequiredOnAuthorisation = false;

            client.purchaseOrderRequiredOnAuthorisationSpecified = true;
            client.purchaseOrderRequiredOnAuthorisation = false;

            client.usesApplicationForPaymentSpecified = true;
            client.usesApplicationForPayment = false;

            client.whtGrossUpSpecified = false;
            client.whtHideOnInvoicesSpecified = false;

            client.companyNo = ClientRef;
            client.companyVatNo = VatNo;
            client.externalId = ClientRef;
            
            client.invoiceDeliveryMethodSpecified = true;
            client.invoiceDeliveryMethod = Mappers.MapInvoiceDeliveryMethod(InvoiceDeliveryMethod);

            client.invoicePeriodSpecified = true;
            client.invoicePeriod = 0;

            client.name = ClientName;
            client.paperOnInvoicesSpecified = true;
            client.paperOnInvoices = 7;

            client.termsDaysSpecified = true;
            client.termsDays = 7;

            //TODO: set terms template on client when set up in RSM
            //client.termsTemplateName = "";

            client.termsType = "Day From Invoice Date";
            
            client.timesheetsOnInvoicesSpecified = true;
            client.timesheetsOnInvoices = 0;

            //TODO: set vat code on client when set up in RSM
            //client.vatCode = "20% VAT";

            client.defaultContractedHoursSpecified = true;
            client.defaultContractedHours = 40;


            //TODO: set default expense template on client once set up in RSM
            //client.defaultExpenseTemplate = "";

            client.defaultTimesheetDateCalculator = "WEEKLY";

            return client;
        }

        //private void DefaultValues(Sti.Client client)
        //{
        //    client.Public_SectorTF = TrueFalse.F;
        //    client.AllowOnlineExpenses = YesNo.N;
        //    client.ClientToAuthoriseOnlineExpenses = YesNo.N;
        //    client.HideClientChargeRates = TrueFalse.F;
        //    client.Create_TS_Image_On_Transfer = TrueFalse.T;
        //}

        /*
        private void MapInvoiceAddresses(Sti.Client client)
        {
            //map invoice addresses if there are any
            if (InvoiceAddresses != null && InvoiceAddresses.Any())
            {
                client.InvoiceAddresses = new List<ClientAddress>();
                foreach (var address in InvoiceAddresses)
                {
                    var ia = address.MapClientAddress(ClientRef, null, null);
                    ia.AddressName = ClientName;
                    ia.Department = Unit.FinanceCode;
                    ia.EmployerRef = client.EmployerRef;
                    client.InvoiceAddresses.Add(ia);
                }
            }
        }*/

        //private void MapInvoiceDeliveryMethod(Sti.Client client)
        //{
        //    if (InvoiceDeliveryMethod == null)
        //    {
        //        client.InvoicingEmail = string.Empty;
        //        return;
        //    }

        //    client.InvoicingEmail = InvoiceEmail;

        //    if (!string.IsNullOrEmpty(InvoiceEmail2))
        //        client.InvoicingEmail += "; " + InvoiceEmail2;

        //    if (!string.IsNullOrEmpty(InvoiceEmail3))
        //        client.InvoicingEmail += "; " + InvoiceEmail3;

        //    switch (InvoiceDeliveryMethod)
        //    {
        //        case "Electronic":
        //        {
        //            client.InvoiceDeliveryMethod = ClientInvoiceDeliveryMethod.El;
        //            break;
        //        }
        //        case "Paper":
        //        {
        //            client.InvoiceDeliveryMethod = ClientInvoiceDeliveryMethod.Pr;
        //            client.InvoicingEmail = string.Empty;
        //            break;
        //        }

        //        case "Self Bill":
        //        {
        //            client.InvoiceDeliveryMethod = ClientInvoiceDeliveryMethod.Bo;
        //            client.InvoicingEmail = string.Empty;
        //            break;
        //        }
                
        //        default:
        //            throw new Exception("Unknown Invoice Delivery Method");
        //    }


        //}
    }
}


