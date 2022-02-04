using System;
using System.Collections.Generic;
using System.Text;

namespace Randstad.UfoRsm.BabelFish.Template.Extensions
{
    public static class GetDateMillisecondsExtension
    {
        public static long GetDateMilliseconds(this DateTime? date)
        {
            var d = (DateTime)date;
            var b = DateTime.Parse(d.ToString("yyyy-MM-dd 00:00:00"));
            var epochTicks = new DateTime(1970, 1, 1).Ticks;

            var unixTime = (b.Date.Ticks - epochTicks) / TimeSpan.TicksPerMillisecond;
            return unixTime;
        }
    }
}
