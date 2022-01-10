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
        public string ClientName { get; set; }

        public string VatNo { get; set; }
        public string InvoiceDeliveryMethod { get; set; }

        public bool? IsCheckedIn { get; set; }

        public Address WorkAddress { get; set; }

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

    }
}


