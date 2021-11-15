namespace Events.Bot.Common.RequestResult
{
    public interface IRequestResult<T>
    {
        public T Value { get; set; }
        public RequestResultType Type { get; set; }
    }
}
