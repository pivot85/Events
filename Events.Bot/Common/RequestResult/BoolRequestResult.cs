namespace Events.Bot.Common.RequestResult
{
    public class BoolRequestResult : IRequestResult<bool>
    {
        public bool Value { get; set; }
        public RequestResultType Type { get; set; }
    }
}