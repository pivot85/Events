using System;

namespace Events.Bot.Common.RequestResult
{
    public class DateTimeRequestResult : IRequestResult<DateTime>
    {
        public DateTime Value { get; set; }
        public RequestResultType Type { get; set; }
    }
}