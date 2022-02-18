using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Template.Extensions
{
    public static class GetDateMillisecondsExtension
    {

        public static long GetDateTimeMilliseconds(this DateTime? date)
        {
            var centuryBegin = new DateTime(1970,1, 1);
            var currentDate = (DateTime)date;

            var elapsedTicks = (currentDate.Ticks - centuryBegin.Ticks) / TimeSpan.TicksPerMillisecond;

            //DateTimeOffset now = DateTimeOffset.UtcNow;
            //long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();

            return elapsedTicks;
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
