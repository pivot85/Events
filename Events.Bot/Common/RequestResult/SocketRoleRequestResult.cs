using Discord.WebSocket;

namespace Events.Bot.Common.RequestResult
{
    public class SocketRoleRequestResult : IRequestResult<SocketRole>
    {
        public SocketRole Value { get; set; }
        public RequestResultType Type { get; set; }
    }
}