using System;

namespace Events.Bot.Extensions
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimeSeconds(this DateTime dateTime) => ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }
}
