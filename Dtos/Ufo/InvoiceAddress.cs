using System;
using System.Collections.Generic;
using System.Linq;
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
            var addressLines = new List<string>();

            addressLines.AddRange(TruncateField(Fao));
            addressLines.AddRange(TruncateField(Street));
            addressLines.AddRange(TruncateField(City));
            addressLines.AddRange(TruncateField(County));
            addressLines.AddRange(TruncateField(Country));

            var country = string.Empty;
            for (var x = 0; x < addressLines.Count; x++)
            {
                switch (x)
                {
                    case 0:
                    {
                        address.line1 = addressLines[x];
                        break;
                    }
                    case 1:
                    {
                        address.line2 = addressLines[x];
                        break;
                    }
                    case 2:
                    {
                        address.town = addressLines[x];
                        break;
                    }
                    case 3:
                    {
                        address.county = addressLines[x];
                        break;
                    }
                    default:
                    {
                        country = country + addressLines[x] + ", ";
                        break;
                    }
                }
            }

            if (country != string.Empty)
            {
                address.country = country.Remove(country.LastIndexOf(","));
            }

            address.postcode = PostCode;
            return address;
        }

        private List<string> TruncateField(string addressField)
        {
            var list = new List<string>();

            if (string.IsNullOrEmpty(addressField)) return list;

            List<string> split;

            //if there are commas in the addressfield then split on that and truncate each line
            if (addressField.Contains(","))
            {
                split = addressField.Split(',').ToList();

                foreach (var x in split)
                {
                    list.AddRange(splitLine(x));
                }
            }
            else
            {
                list.AddRange(splitLine(addressField));
            }

            return list;


        }

        private List<string> splitLine(string line)
        {
            List<string> split = new List<string>();

            if (line.Length <= 35)
            {
                split.Add(line);
                return split;
            }

            var temp = string.Empty;

            temp = line.Substring(0, 35);

            var lastWholeWordIndex = temp.LastIndexOf(" ");

            if (lastWholeWordIndex > 0)
            {
                temp = temp.Substring(0, lastWholeWordIndex);
            }

            split.Add(temp);
            split.Add(line.Remove(0, temp.Length).Trim());

            return split;
        }

        
    }
}
