using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class ClientContact : ObjectBase
    {
        public string ContactId { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string Department { get; set; }
        public Team OpCo { get; set; }
        public Address MailingAddress { get; set; }

        public bool IsCheckedIn { get; set; }

        public RSM.Contact MapContact()
        {
            var contact = new RSM.Contact();
            contact.department = Department;
            contact.email = EmailAddress;
            contact.externalId = ContactId;
            contact.firstname = Forename;
            contact.lastname = Surname;
            if (MailingAddress != null)
            {
                contact.address = MailingAddress.GetAddress();
            }

            return contact;
        }

        public RSM.Manager MapContactManager(Client client)
        {
            var manager = new RSM.Manager();
            manager.email = EmailAddress;
            manager.username = EmailAddress;
            manager.firstname = Forename;
            manager.lastname = Surname;
            manager.refCode = ContactId;
            manager.clientExternalId = client.ClientId;
            manager.externalId = ContactId;
            manager.commsDisabledSpecified = true;
            manager.commsDisabled = true;
            return manager;
        }


    }
}
