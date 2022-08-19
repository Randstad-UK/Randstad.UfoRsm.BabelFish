using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Randstad.Logging;
using Randstad.OperatingCompanies;
using Randstad.UfoRsm.BabelFish.Helpers;
using RSM;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Client : ObjectBase
    {
        public string ClientId { get; set; }
        public string ClientRef { get; set; }
        public string ClientName { get; set; }
        public bool? NoVat { get; set; }

        public string VatNo { get; set; }
        public string CompanyRegNum { get; set; }
        public decimal? CreditLimit { get; set; }
        public string InvoiceDeliveryMethod { get; set; }
        public string InvoiceEmail { get; set; }
        public string InvoiceEmail2 { get; set; }
        public string InvoiceEmail3 { get; set; }

        public bool? IsCheckedIn { get; set; }

        public Address WorkAddress { get; set; }

        public Team OpCo { get; set; }

        public Team Unit { get; set; }

        public Client HleClient { get; set; }
        public string PoRequired { get; set; }

        public RSM.Client MapClient()
        {
            var client = new RSM.Client();
            
            client.accountsRef = ClientRef;

            client.customText1 = HleClient != null ? HleClient.ClientRef : ClientRef;

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


            client.companyNo = HleClient != null ? HleClient.CompanyRegNum : CompanyRegNum;
            
            if (client.companyNo == null)
                client.companyNo = "";

            client.customText2 = HleClient != null ? "" : CreditLimit.ToString();
            
            if (client.customText2 == null)
                client.customText2 = "";

            client.companyVatNo = HleClient !=null ? HleClient.VatNo : VatNo;

            if (client.companyVatNo == null)
                client.companyVatNo = "";
            
            client.externalId = ClientRef;
            
            client.invoiceDeliveryMethodSpecified = true;
            client.invoiceDeliveryMethod = Mappers.MapInvoiceDeliveryMethod(InvoiceDeliveryMethod);

            client.invoicePeriodSpecified = true;
            client.invoicePeriod = 0;

            client.name = ClientName;

            if (PoRequired != null)
            {
                client.invoiceRequiresPOSpecified = true;
                client.invoiceRequiresPO = Mappers.MapBool(PoRequired);
            }

            //client name in RSM has a max length of 90 characters so truncate it
            if (client.name.Length > 80)
            {
                client.name = client.name.Substring(0, 79);
            }

            client.paperOnInvoicesSpecified = true;
            client.paperOnInvoices = 7;
            
            client.termsDaysSpecified = true;
            client.termsDays = 14;

            //TODO: (Done) set terms template on client when set up in RSM
            client.termsTemplateName = "Default Charge Terms";

            client.vatCode = "T1";

            if (HleClient != null)
            {
                if (HleClient.NoVat != null && HleClient.NoVat == true)
                {
                    client.vatCode = "T0";
                }
            }
            else
            {
                if (NoVat != null && NoVat == true)
                {
                    client.vatCode = "T0";
                }
            }

            client.termsType = "Days From Invoice Date";
            
            client.timesheetsOnInvoicesSpecified = true;
            client.timesheetsOnInvoices = 0;

            client.defaultContractedHoursSpecified = true;
            client.defaultContractedHours = 40;


            client.defaultTimesheetDateCalculator = "weekly";

            client.primaryContact = new Contact();

            if (WorkAddress != null)
            {
                client.primaryContact.address = WorkAddress.GetAddress();
            }

            return client;
        }

    }
}


