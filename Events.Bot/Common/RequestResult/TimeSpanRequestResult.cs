using System;

namespace Events.Bot.Common.RequestResult
{
    public class TimeSpanRequestResult : IRequestResult<TimeSpan>
    {
        public TimeSpan Value { get; set; }
        public RequestResultType Type { get; set; }
    }
}