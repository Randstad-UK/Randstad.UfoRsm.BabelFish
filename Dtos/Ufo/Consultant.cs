using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Randstad.UfoRsm.BabelFish.Helpers;

namespace Randstad.UfoRsm.BabelFish.Dtos.Ufo
{
    public class Consultant : ObjectBase
    {
        public string Id { get; set; }
        public string MigratedUserId { get; set; }
        public string EmployeeRef { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public Team OpCo { get; set; }

        public RSM.Consultant MapConsultant()
        {

            var consultant = new RSM.Consultant();

            consultant.canLoginSpecified = true;
            consultant.canLogin = IsActive;
            
            consultant.commsDisabledSpecified = true;
            consultant.commsDisabled = true;

            consultant.department = Unit.FinanceCode;
            consultant.email = Email;
            consultant.externalId = "UFO"+EmployeeRef;
            consultant.firstname = Firstname;
            consultant.lastname = Lastname;
            consultant.team = Branch.Name;

            return consultant;
        }
    }
}
