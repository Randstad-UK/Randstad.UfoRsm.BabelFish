using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Randstad.UfoSti.BabelFish.Dtos.Ufo
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
        public string OtherPhone { get; set; }
        public Owner Owner { get; set; }
        public bool? HasLeft { get; set; }
        public Client ContactClient { get; set; }
        public ClientContactRelationship RelatedClientRelationship { get; set; }

        public string ContactType { get; set; }
        public bool IsCheckedIn { get; set; }

        public Sti.Client MapClient(string consultantPrefixCode, Dictionary<string, string> tomCodes)
        {
            
            //map the contacts client first
            Sti.Client client = null;
            /*
            Sti.Client hle = null;

            if (ContactClient == null)
                client = new Sti.Client();
            else
                client = ContactClient.MapClient(consultantPrefixCode, out hle, tomCodes);

            client.ClientTelephone = Phone;
            client.ClientMobileTelephone = Mobile;
            client.ClientEmail = EmailAddress;
            */

            return client;
        }


    }
}
