using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Template.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertToBST(this DateTime? date)
        {
            var d = (DateTime) date;

            return d.ToLocalTime();
        }


    }
}
