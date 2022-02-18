using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }

        public RSM.Address GetAddress()
        {
            var addr = new RSM.Address();

            TruncateAddressStreet(addr);
            addr.town = City;
            addr.county = County;
            addr.country = Country;
            addr.postcode = PostCode;

            return addr;
        }

        public string GetConcatenatedAddress()
        {
            var sb = new StringBuilder();
            sb.Append(Street);

            if (!string.IsNullOrEmpty(City))
            {
                sb.Append(", ").Append(City);
            }

            if (!string.IsNullOrEmpty(County))
            {
                sb.Append(", ").Append(County);
            }

            if (!string.IsNullOrEmpty(Country))
            {
                sb.Append(", ").Append(Country);
            }

            if (!string.IsNullOrEmpty(PostCode))
            {
                sb.Append(", ").Append(PostCode);
            }

            return sb.ToString();
        }

        private void TruncateAddressStreet(RSM.Address address)
        {

            var streetTemp = Street;

            if (string.IsNullOrEmpty(streetTemp)) return;

            var temp = string.Empty;

            if (streetTemp.Length > 35)
                temp = streetTemp.Substring(0, 35);
            else
            {
                address.line1 = Street;
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
