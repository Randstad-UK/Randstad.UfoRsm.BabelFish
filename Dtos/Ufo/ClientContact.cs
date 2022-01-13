﻿using System;
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
        public string MigratedPersonId { get; set; }
        public string MigratedClientContactId { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Department { get; set; }
        public string OtherPhone { get; set; }
        public Owner Owner { get; set; }
        public bool? HasLeft { get; set; }
        public Client ContactClient { get; set; }
        public Address Address { get; set; }

        public string ContactType { get; set; }
        public bool IsCheckedIn { get; set; }

        public RSM.Contact MapInvoiceContact()
        {
            var contact = new RSM.Contact();
            contact.department = Department;
            contact.email = EmailAddress;
            contact.externalId = ContactId;
            contact.firstname = Forename;
            contact.lastname = Surname;
            return null;
        }


    }
}