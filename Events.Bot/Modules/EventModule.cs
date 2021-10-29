using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Events.Bot.Utils;
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
                    await Context.Channel.SendMessageAsync("You didn't respond in time, please run the command again.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await Context.Channel.SendMessageAsync("The setup was cancelled.");
                    return;
                }

                if (content.Length > TITLE_CHAR_LIMIT)
                {
                    await Context.Channel.SendMessageAsync($"Please pick a title that is shorter than {TITLE_CHAR_LIMIT} characters.");
                    continue;
                }

                title = content;
                break;
            }

            await Context.Channel.SendMessageAsync($"Title set! What description would you like the event to have? It must be shorter than {DESC_CHAR_LIMIT} characters.");
            string description = string.Empty;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await Context.Channel.SendMessageAsync("You didn't respond in time, please run the command again.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await Context.Channel.SendMessageAsync("The setup was cancelled.");
                    return;
                }

                if (content.Length > DESC_CHAR_LIMIT)
                {
                    await Context.Channel.SendMessageAsync($"Please pick a description that is shorter than {DESC_CHAR_LIMIT} characters.");
                    continue;
                }

                description = content;
                break;
            }

            await Context.Channel.SendMessageAsync("Description set! When should the event take place? Please format it as mm/dd/yyyy HH:MM:SS");
            DateTime start;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await Context.Channel.SendMessageAsync("You didn't respond in time, please run the command again.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await Context.Channel.SendMessageAsync("The setup was cancelled.");
                    return;
                }

                if (!DateTime.TryParse(content, out start))
                {
                    await Context.Channel.SendMessageAsync($"Please provide a properly formatted date and time for the start of the event.");
                    continue;
                }
                break;
            }

            await Context.Channel.SendMessageAsync($"The event will start on {string.Format("{0:f}", start)}. How long will the event last? Please provide the duration as HH:MM:SS.");
            TimeSpan duration;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await Context.Channel.SendMessageAsync("You didn't respond in time, please run the command again.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await Context.Channel.SendMessageAsync("The setup was cancelled.");
                    return;
                }

                if (!TimeSpan.TryParse(content, out duration))
                {
                    await Context.Channel.SendMessageAsync($"Please provide a properly formatted duration for the event.");
                    continue;
                }
                break;
            }

            var eventId = Guid.NewGuid();
            var eventIdSubstring = eventId.ToString().Substring(0, 7);

            await Context.Channel.SendMessageAsync($"The event will last for {(duration.TotalHours > 1 ? $"{duration.Hours} hour(s) and {duration.Minutes} minute(s)" : $"{duration.Minutes} minutes")}. Do you want to add any stewards? Please mention them or respond with \"skip\".");
            ulong stewardRoleId = 0;
            RestRole stewardRole = null;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await Context.Channel.SendMessageAsync("You didn't respond in time, cancelled the setup.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await Context.Channel.SendMessageAsync("The setup was cancelled.");
                    return;
                }

                stewardRole = await Context.Guild.CreateRoleAsync(eventIdSubstring + " Steward", isHoisted: false, isMentionable: false);
                stewardRoleId = stewardRole.Id;

                if (content.ToLower() == "skip")
                    break;

                var users = new List<SocketGuildUser>();
                foreach (var mention in content.Split(" "))
                {
                    if (!MentionUtils.TryParseUser(mention, out ulong userId))
                        continue;

                    var user = Context.Guild.GetUser(userId);
                    if (user == null || users.Contains(user))
                        continue;

                    await user.AddRoleAsync(stewardRole);
                    users.Add(user);
                }

                break;
            }

            await Context.Channel.SendMessageAsync($"Stewards have been set! Do you want to add any speakers? Please mention them or respond with \"skip\".");
            ulong speakerRoleId = 0;
            RestRole speakerRole = null;
            while (true)
            {
                var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                if (response.IsTimeouted || response.Value == null)
                {
                    await Context.Channel.SendMessageAsync("You didn't respond in time, cancelled the setup.");
                    return;
                }

                string content = response.Value.Content;

                if (content.ToLower() == "cancel")
                {
                    await stewardRole.DeleteAsync();
                    await Context.Channel.SendMessageAsync("The setup was cancelled.");
                    return;
                }

                speakerRole = await Context.Guild.CreateRoleAsync(eventIdSubstring + " Speaker", isHoisted: false, isMentionable: false);
                speakerRoleId = stewardRole.Id;

                if (content.ToLower() == "skip")
                    break;

                var users = new List<SocketGuildUser>();
                foreach (var mention in content.Split(" "))
                {
                    if (!MentionUtils.TryParseUser(mention, out ulong userId))
                        continue;

                    var user = Context.Guild.GetUser(userId);
                    if (user == null || users.Contains(user))
                        continue;

                    await user.AddRoleAsync(speakerRole);
                    users.Add(user);
                }

                break;
            }

            ulong cosmeticRoleId = 0;
            if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync($"Is there a role you'd like to use as a cosmetic role? Please mention it or respond with \"skip\".");
                while (true)
                {
                    var response = await Interactivity.NextMessageAsync(x => x.Author.Id == Context.User.Id);
                    if (response.IsTimeouted || response.Value == null)
                    {
                        await Context.Channel.SendMessageAsync("You didn't respond in time, cancelled the setup.");
                        return;
                    }

                    string content = response.Value.Content;

                    if (content.ToLower() == "cancel")
                    {
                        await stewardRole.DeleteAsync();
                        await speakerRole.DeleteAsync();
                        await Context.Channel.SendMessageAsync("The setup was cancelled.");
                        return;
                    }

                    if (content.ToLower() == "skip")
                        break;

                    if (!MentionUtils.TryParseRole(content, out ulong roleId))
                    {
                        await Context.Channel.SendMessageAsync($"Please provide a valid mention of a role.");
                        continue;
                    }

                    var role = Context.Guild.GetRole(roleId);
                    if (role == null)
                    {
                        await Context.Channel.SendMessageAsync("That role does not exist, please provide a valid role.");
                        continue;
                    }

                    cosmeticRoleId = role.Id;
                    break;
                }
            }

            var message = await Context.Channel.SendMessageAsync("All set! Time to create the event...");
            var attendeeRole = await Context.Guild.CreateRoleAsync(eventIdSubstring + " Attendee", isHoisted: false, isMentionable: false);

            var category = await Context.Guild.CreateCategoryChannelAsync("Event " + eventIdSubstring);
            var textChannel = await Context.Guild.CreateTextChannelAsync(eventIdSubstring + "-general", x => x.CategoryId = category.Id);
            var voiceChannel = await Context.Guild.CreateVoiceChannelAsync("Event " + eventIdSubstring, x => x.CategoryId = category.Id);
            var controlChannel = await Context.Guild.CreateTextChannelAsync(eventIdSubstring + "-control", x => x.CategoryId = category.Id);

            await EventsDataAccessLayer.Create(eventId, Context.Guild.Id, Context.User.Id, title, start, duration, category.Id, textChannel.Id, voiceChannel.Id, controlChannel.Id, stewardRoleId, speakerRoleId, attendeeRole.Id, cosmeticRoleId, false);

            await message.ModifyAsync(x => x.Content = $"Done! All roles and channels have been created for the event.");
        }
    }
}
