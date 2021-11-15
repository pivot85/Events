using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToHoursMinutes(this TimeSpan timeSpan)
        {
            return $"{(timeSpan.Hours >= 1 ? $"{timeSpan.Hours} {(timeSpan.Hours > 1 ? "hours" : "hour")}" : "")}" +
                $"{(timeSpan.Hours >= 1 && timeSpan.Minutes >= 1 ? " and " : "")}" +
                $"{(timeSpan.Minutes > 0 ? $"{timeSpan.Minutes} {(timeSpan.Minutes > 1 ? "minutes" : "minute")}" : "")}";
        }
    }
}
