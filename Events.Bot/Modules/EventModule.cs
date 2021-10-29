using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Events.Data.DataAccessLayer;
using Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Events.Bot.Modules
{
    public class EventModule : DualModuleBase
    {
        public EventModule(EventsDataAccessLayer eventsDataAccessLayer, PermittedRolesDataAccessLayer permittedRoleDataAccessLayer, InteractivityService interactivityService)
            : base(eventsDataAccessLayer, permittedRoleDataAccessLayer, interactivityService)
        {
        }

        private readonly int TITLE_CHAR_LIMIT = 128;
        private readonly int DESC_CHAR_LIMIT = 1536;

        [Command("newevent")]
        public async Task NewEventAsync()
        {
            var permittedRoles = await PermittedRoleDataAccessLayer.GetAllByGuild(Context.Guild.Id);
            if (!await UserIsPermitted())
            {
                await ReplyAsync("Not allowed!");
                return;
            }

            await ReplyAsync($"Let's create a new event. If you want to cancel at any time, you can respond with \"cancel\". First, what would you like the title to be? The title must be shorter than {TITLE_CHAR_LIMIT} characters.");
            string title = string.Empty;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await ReplyAsync("You didn't respond in time, please run the command again.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await ReplyAsync("The setup was cancelled.");
                    return;
                }

                if (content.Length > TITLE_CHAR_LIMIT)
                {
                    await ReplyAsync($"Please pick a title that is shorter than {TITLE_CHAR_LIMIT} characters.");
                    continue;
                }

                title = content;
                break;
            }

            await ReplyAsync($"Title set! What description would you like the event to have? It must be shorter than {DESC_CHAR_LIMIT} characters.");
            string description = string.Empty;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await ReplyAsync("You didn't respond in time, please run the command again.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await ReplyAsync("The setup was cancelled.");
                    return;
                }

                if (content.Length > DESC_CHAR_LIMIT)
                {
                    await ReplyAsync($"Please pick a description that is shorter than {DESC_CHAR_LIMIT} characters.");
                    continue;
                }

                description = content;
                break;
            }

            await ReplyAsync("Description set! When should the event take place? Please format it as mm/dd/yyyy HH:MM:SS");
            DateTime start;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await ReplyAsync("You didn't respond in time, please run the command again.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await ReplyAsync("The setup was cancelled.");
                    return;
                }

                if (!DateTime.TryParse(content, out start))
                {
                    await ReplyAsync($"Please provide a properly formatted date and time for the start of the event.");
                    continue;
                }
                break;
            }

            await ReplyAsync($"The event will start on {string.Format("{0:f}", start)}. How long will the event last? Please provide the duration as HH:MM:SS.");
            TimeSpan duration;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await ReplyAsync("You didn't respond in time, please run the command again.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await ReplyAsync("The setup was cancelled.");
                    return;
                }

                if (!TimeSpan.TryParse(content, out duration))
                {
                    await ReplyAsync($"Please provide a properly formatted duration for the event.");
                    continue;
                }
                break;
            }

            //await ReplyAsync($"The event will last for {(duration.TotalHours > 0 ? $"{duration.Hours} hour(s) and {duration.Minutes} minute(s)" : $"{duration.Minutes} minutes")}. Do you want to add any stewards? Please mention them or respond with \"skip\".");
            //ulong stewardRole;
            //while (true)
            //{
            //    var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
            //    if (response.IsTimeouted || response.Value == null)
            //    {
            //        await ReplyAsync("You didn't respond in time, cancelled the setup.");
            //        return;
            //    }

            //    string content = response.Value.Content;

            //    if (content.ToLower() == "cancel")
            //    {
            //        await ReplyAsync("The setup was cancelled.");
            //        return;
            //    }

            //    if (content.ToLower() == "skip")
            //        break;

            //    var users = new List<SocketGuildUser>();
            //    foreach (var mention in content.Split(" "))
            //    {
            //        if (!MentionUtils.TryParseUser(mention, out ulong userId))
            //            continue;

            //        var user = Context.Guild.GetUser(userId);
            //        if (user == null || users.Contains(user))
            //            continue;

            //        if (stewardRole == null)
            //        {
            //            var newRole = await Context.Guild.CreateRoleAsync(title.Take(8))
            //        }

            //        allowedRoles.Add(role.Id);
            //    }

            //    if (allowedRoles.Count == 0)
            //    {
            //        await ReplyAsync("There were no (valid) roles provided, please try again.");
            //        continue;
            //    }

            //    break;
            //}
        }
    }
}
