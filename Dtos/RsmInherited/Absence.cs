using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.RsmInherited
{
    public class Absence
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Decimal NoOfUnits { get; set; }
        public string EmployeeNo { get; set; }
        public int AbsenceType { get; set; }
    }
}
