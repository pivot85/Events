namespace Events.Bot.Common.RequestResult
{
    public class StringRequestResult : IRequestResult<string>
    {
        public string Value { get; set; }
        public RequestResultType Type { get; set; }
    }
}