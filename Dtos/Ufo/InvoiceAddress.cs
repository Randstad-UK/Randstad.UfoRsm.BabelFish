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

        public RSM.Contact MapContact()
        {
            var contact = new RSM.Contact();
            contact.address = new RSM.Address();
            contact.address.line1 = Street;
            contact.address.town = City;
            contact.address.county = County;
            contact.address.country = Country;
            contact.address.postcode = PostCode;

            return contact;
        }

        /*
        public List<Sti.ClientAddress> MapClientAddress()
        {
            List<ClientAddress> clientAddressList = new List<ClientAddress>();


            foreach(Client c in Client)
            {

                var clientAddress = new Sti.ClientAddress();
                clientAddress.InvoiceAddress = YesNo.Y;
                clientAddress.ClientRef = c.ClientRef;
                clientAddress.Department = c.Unit.FinanceCode;

                //populate address name with the client name unless it's got a different legal hirer
                clientAddress.AddressName = c.ClientName;

                if (c.HleClient!=null && c.HleClient.ClientRef != c.ClientRef)
                {
                    clientAddress.AddressName = c.HleClient.ClientName;
                }

                
                clientAddress.EntityReference = InvoiceAddressRef;

                //****THIS NEEDS FIXED****
                //clientAddress.ContactName = Fao;

                //if the opco is cpe then append HLE client name if it exists and then truncate

                clientAddress.AddressLine1 = Fao;
                TruncateAddressStreet(clientAddress);
                clientAddress.AddressLine4 = City;
                clientAddress.AddressLine5 = County+ " " +Country;
                clientAddress.Postcode = PostCode;

                if (!string.IsNullOrEmpty(c.InvoiceAddressNo))
                {
                    clientAddress.AddressNumber = int.Parse(c.InvoiceAddressNo);
                }

                clientAddressList.Add(clientAddress);
            }


            return clientAddressList;

        }*/

        /*
        public Sti.ClientAddress MapSingleClientAddress(string ClientRef, string clientName, string Department)
        {
            var clientAddress = new Sti.ClientAddress();
            clientAddress.InvoiceAddress = YesNo.Y;
            clientAddress.ClientRef = ClientRef;
            clientAddress.Department = Department;

            clientAddress.AddressName = Client[0].ClientName;
            if (Client[0].HleClient != null)
            {
                clientAddress.AddressName = Client[0].HleClient.ClientName;
            }


            clientAddress.EntityReference = InvoiceAddressRef;

            clientAddress.AddressLine1 = Fao;

            TruncateAddressStreet(clientAddress);
            clientAddress.AddressLine4 = City;
            clientAddress.AddressLine5 = County + " " + Country;
            clientAddress.Postcode = PostCode;

            if (!string.IsNullOrEmpty(AddressId))
            {
                clientAddress.AddressNumber = int.Parse(AddressId);
            }
            
            return clientAddress;
        }
        
        

        private void TruncateAddressStreet(Sti.ClientAddress address)
        {

            var streetTemp = Street;

            var addressLines = new List<string>();


            if (!string.IsNullOrEmpty(streetTemp))
            {
                while (1 == 1)
                {
                    var temp = string.Empty;

                    if (streetTemp.Length > 40)
                        temp = streetTemp.Substring(0, 40);
                    else
                    {
                        addressLines.Add(streetTemp);
                        break;
                    }

                    //get to last whole word
                    var a = temp.LastIndexOf(" ");
                    if (a > 0)
                    {
                        temp = temp.Substring(0, a);
                    }

                    addressLines.Add(temp);

                    streetTemp = streetTemp.Remove(0, temp.Length).Trim();
                }
            }

            var count = 1;
            foreach (var s in addressLines)
            {
                switch (count)
                {
                    case 1:
                    {
                        address.AddressLine2 = s;
                        count++;
                        continue;
                    }
                    case 2:
                    {
                        address.AddressLine3 = s;
                        count++;
                        continue;
                    }
                }
            }
        }
        */
    }
}
