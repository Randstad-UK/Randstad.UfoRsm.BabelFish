using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.RsmInherited
{
    public class HolidayRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Decimal Hours { get; set; }
        public string EmployeeNumber { get; set; }
        public int AbsenceType { get; set; }
    }
}
