using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Template.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertToBST(this DateTime? date)
        {
            var info = TimeZoneInfo.FindSystemTimeZoneById("Greenwich Standard Time");
            DateTimeOffset localServerTime = DateTimeOffset.Now;
            var isDaylightSaving = info.IsDaylightSavingTime(localServerTime);

            var d = (DateTime)date;

            return d.ToLocalTime();
        }


    }
}