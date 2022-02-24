using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Randstad.Logging;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class InvoiceAddress
    {
        public string InvoiceAddressId { get; set; }

        public string InvoiceAddressRef { get; set; }
        public string MigratedAddressId { get; set; }
        public string AddressId { get; set; }
        public string Fao { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public List<Client> Client { get; set; }

        public RSM.Address MapAddress()
        {
            var address = new RSM.Address();
            TruncateAddressStreet(address);
            address.town = City;
            address.county = County;
            address.country = Country;
            address.postcode = PostCode;

            return address;
        }

        private void TruncateAddressStreet(RSM.Address address)
        {

            var streetTemp = "";
            if (!string.IsNullOrEmpty(Fao))
            {
                streetTemp = Fao + ", ";
            }

            streetTemp = streetTemp + Street;

            if (string.IsNullOrEmpty(streetTemp)) return;

            var temp = string.Empty;

            if (streetTemp.Length > 35)
                temp = streetTemp.Substring(0, 35);
            else
            {
                address.line1 = streetTemp;
                return;
            }

            //get to last whole word
            var a = temp.LastIndexOf(" ");
            if (a > 0)
            {
                temp = temp.Substring(0, a);
            }

            address.line1 = temp;

            address.line2 = streetTemp.Remove(0, temp.Length).Trim();

        }
        
    }
}
