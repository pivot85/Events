using Discord.Rest;
using System.Collections.Generic;

namespace Events.Bot.Common.RequestResult
{
    public class RestGuildUsersRequestResult : IRequestResult<List<RestGuildUser>>
    {
        public List<RestGuildUser> Value { get; set; }
        public RequestResultType Type { get; set; }
    }
}