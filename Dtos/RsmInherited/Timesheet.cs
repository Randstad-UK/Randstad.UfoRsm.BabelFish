using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Dtos.RsmInherited
{
    public class Timesheet : RSM.Timesheet
    {public List<RSM.ExpenseItem> Expenses { get; set; }
    }
}
