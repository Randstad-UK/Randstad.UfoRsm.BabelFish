using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Template.Extensions
{
    public static class GetDateMillisecondsExtension
    {

        public static long GetDateTimeMilliseconds(this DateTime? date)
        {
            var currentDate = (DateTime)date;
            long eTicks = (long)currentDate.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalMilliseconds;
            return eTicks;
        }

        public static long GetDateTimeMilliseconds(this string date)
        {
            var d = DateTime.Parse(date);
            long eTicks = (long)d.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalMilliseconds;
            return eTicks;
        }

        public static long GetDateMilliseconds(this DateTime? date)
        {
            var centuryBegin = new DateTime(1970, 1, 1);
            DateTime currentDate = (DateTime)date;

            var elapsedTicks = (currentDate.Date.Ticks - centuryBegin.Ticks) / TimeSpan.TicksPerMillisecond;

            return elapsedTicks;
        }
    }
}