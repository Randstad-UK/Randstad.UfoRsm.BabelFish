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

        private void TruncateAddressStreet(RSM.Address address)
        {

            var streetTemp = Street;

            var addressLines = new List<string>();



            while (1 == 1)
            {
                var temp = string.Empty;

                if (streetTemp!=null && streetTemp.Length > 40)
                    temp = streetTemp.Substring(0, 40);
                else
                {
                    addressLines.Add(streetTemp);
                    break;
                }

                //get to last whole word
                temp = temp.Substring(0, temp.LastIndexOf(" "));
                addressLines.Add(temp);

                streetTemp = streetTemp.Remove(0, temp.Length).Trim();
            }

            var count = 1;
            foreach (var s in addressLines)
            {
                switch (count)
                {
                    case 1:
                    {
                        address.line1 = s;
                        count++;
                        continue;
                    }
                    case 2:
                    {
                        address.line2 = s;
                        count++;
                        continue;
                    }
                }
            }
        }
    }
}
