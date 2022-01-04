using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Randstad.UfoSti.BabelFish.Dtos.Ufo
{
    public class ClientContactRelationship
    {
        public string ClientContactId { get; set; }
        public string MigratedClientContactId { get; set; }
        public bool? IsActive { get; set; }
        public Client Client { get; set; }
        public ClientContact ClientContact { get; set; }
        public bool ActiveInSirenum { get; set; }
        public DateTime? SentToSirenum { get; set; }
        public DateTime? DeactivatedInSirenum { get; set; }
        public bool? DirectRelationship { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }



    }
}
