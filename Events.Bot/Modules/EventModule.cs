using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Events.Bot.Utils;
using Events.Data.DataAccessLayer;
using Events.Data.Models;
using Interactivity;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Events.Bot.Modules
{
    public class EventModule : DualModuleBase
    {
        public EventModule(EventsDataAccessLayer eventsDataAccessLayer, PermittedRolesDataAccessLayer permittedRoleDataAccessLayer, InteractivityService interactivityService, IConfiguration configuration)
            : base(eventsDataAccessLayer, permittedRoleDataAccessLayer, interactivityService, configuration)
        {
        }

        private readonly int TITLE_CHAR_LIMIT = 128;
        private readonly int DESC_CHAR_LIMIT = 1536;
        private readonly int SHORT_NAME_CHAR_LIMIT = 16;
        private readonly int REQUIRED_AVAILABLE_ROLES_COUNT = 4;
        private readonly int REQUIRED_AVAILABLE_CHANNELS_COUNT = 3;
        private readonly int GUILD_ROLES_CAP = 250;
        private readonly int GUILD_CHANNELS_CAP = 500;

        // Temporary clean-up command for testing purposes
        [Command("clear")]
        public async Task ClearAsync()
        {
            var message = await ReplyAsync("Initiating clean-up...");

            var categories = Context.Guild.CategoryChannels.Where(x => x.Name.Contains("Event"));
            foreach (var category in categories)
            {
                foreach (var channel in category.Channels)
                {
                    await channel.DeleteAsync();
                }

                await category.DeleteAsync();
            }

            var roles = Context.Guild.Roles.Where(x => x.Name.Contains("Attendee") || x.Name.Contains("Speaker") || x.Name.Contains("Steward"));
            foreach (var role in roles)
            {
                await role.DeleteAsync();
            }

            var eventsChannel = Context.Guild.GetTextChannel(Configuration.GetValue<ulong>("Events"));
            var messages = await eventsChannel.GetMessagesAsync(100).FlattenAsync();
            await eventsChannel.DeleteMessagesAsync(messages);

            await Context.Channel.SendMessageAsync("Clean-up finished!");
        }

        [Command("new")]
        public async Task NewEventAsync()
        {
            var permittedRoles = await PermittedRoleDataAccessLayer.GetAllByGuild(Context.Guild.Id);
            if (!await UserIsPermitted())
            {
                await ReplyAsync("You are not allowed to create new events. Only administrators and permitted roles can.");
                return;
            }

            if (Context.Guild.Channels.Count() > (GUILD_CHANNELS_CAP - REQUIRED_AVAILABLE_CHANNELS_COUNT) ||
                Context.Guild.Roles.Count() > (GUILD_ROLES_CAP - REQUIRED_AVAILABLE_ROLES_COUNT))
            {
                await ReplyAsync("This server is nearing the maximum amount of roles or channels, thus a new event cannot be created.");
                return;
            }

            await ReplyAsync($"Let's create a new event. If you want to cancel at any time, you can respond with \"cancel\". First, what would you like the title to be? The title must be shorter than {TITLE_CHAR_LIMIT} characters.");
            var title = await Ask<string>(max: TITLE_CHAR_LIMIT, minMaxError: $"Please provide a title that is shorter than or equal to {TITLE_CHAR_LIMIT} characters.");
            if (title == null)
                return;

            await Context.Channel.SendMessageAsync($"Title set! What description would you like the event to have? It must be shorter than {DESC_CHAR_LIMIT} characters.");
            string description = await Ask<string>(max: DESC_CHAR_LIMIT, minMaxError: $"Please pick a description that is shorter than {DESC_CHAR_LIMIT} characters.");
            if (description == null)
                return;

            await Context.Channel.SendMessageAsync("Description set! What would you like the short name of this event to be? This will appear in the channels. This can contain a maximum of 16 characters.");
            string shortName = await Ask<string>(max: SHORT_NAME_CHAR_LIMIT, minMaxError: $"Please provide a short name containing less than or equal to {SHORT_NAME_CHAR_LIMIT} characters.");
            if (shortName == null)
                return;

            await Context.Channel.SendMessageAsync("All set. When should the event take place? Please format it as mm/dd/yyyy HH:MM:SS");
            var start = await Ask<DateTime>(minMaxError: "Please provide a properly formatted date and time for the start of the event.");
            if (start.Year == 1)
                return;

            await Context.Channel.SendMessageAsync($"The event will start on {string.Format("{0:g}", start)}. How long will the event last? Please provide the duration as HH:MM:SS.");
            var duration = await Ask<TimeSpan>(10, 24, "Please provide a duration that is between 10 minutes and 24 hours.", parseFailedMessage: "Please provide a duration formatted as HH:MM:SS.");
            if (duration == TimeSpan.Zero)
                return;

            string durationFormatted = $"{(duration.Hours >= 1 ? $"{duration.Hours} {(duration.Hours > 1 ? "hours" : "hour")}" : "")}{(duration.Hours >= 1 && duration.Minutes >= 1 ? " and " : "")}{(duration.Minutes > 0 ? $"{duration.Minutes} {(duration.Minutes > 1 ? "minutes" : "minute")}" : "")}";

            await Context.Channel.SendMessageAsync($"The event will last for {durationFormatted}. Do you want to add any stewards? Please mention them or respond with \"skip\".");
            var stewardRole = await Context.Guild.CreateRoleAsync(shortName + " Steward", isHoisted: false, isMentionable: false);
            var stewards = await Ask<List<RestGuildUser>>();

            // TODO: only delete when response was cancel
            if (stewards.Count == 0)
            {
                await stewardRole.DeleteAsync();
                return;
            }

            await Context.Channel.SendMessageAsync($"Stewards have been set! Do you want to add any speakers? Please mention them or respond with \"skip\".");
            var speakerRole = await Context.Guild.CreateRoleAsync(shortName + " Speaker", isHoisted: false, isMentionable: false);
            var speakers = await Ask<List<RestGuildUser>>();

            // TODO: only delete when response was cancel
            if (speakers.Count == 0)
            {
                await stewardRole.DeleteAsync();
                await speakerRole.DeleteAsync();
                return;
            }

            ulong cosmeticRoleId = 0;
            if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                await Context.Channel.SendMessageAsync($"Is there a role you'd like to use as a cosmetic role? Please mention it or respond with \"skip\".");
                cosmeticRoleId = (await Ask<SocketRole>()).Id;
            }

            var message = await Context.Channel.SendMessageAsync("All set! Time to create the event...");
            try
            {
                var attendeeRole = await Context.Guild.CreateRoleAsync(shortName + " Attendee", isHoisted: false, isMentionable: false);
                var hostRole = await Context.Guild.CreateRoleAsync(shortName + " Host", isHoisted: false, isMentionable: false);

                var categoryOverwrites = new List<Overwrite>();
                categoryOverwrites.Add(new Overwrite(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
                categoryOverwrites.Add(new Overwrite(hostRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)));
                categoryOverwrites.Add(new Overwrite(stewardRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)));
                categoryOverwrites.Add(new Overwrite(speakerRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)));
                categoryOverwrites.Add(new Overwrite(attendeeRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)));

                var category = await Context.Guild.CreateCategoryChannelAsync("Event " + shortName, x => x.PermissionOverwrites = categoryOverwrites);
                var textChannel = await Context.Guild.CreateTextChannelAsync(shortName + "-general", x => x.CategoryId = category.Id);
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync("Event " + shortName, x => x.CategoryId = category.Id);
                var controlChannel = await Context.Guild.CreateTextChannelAsync(shortName + "-control", x => x.CategoryId = category.Id);

                await textChannel.AddPermissionOverwriteAsync(attendeeRole, new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Deny));
                await voiceChannel.AddPermissionOverwriteAsync(attendeeRole, new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Deny, speak: PermValue.Deny, stream: PermValue.Deny));
                await voiceChannel.AddPermissionOverwriteAsync(hostRole, new OverwritePermissions(viewChannel: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow));
                await voiceChannel.AddPermissionOverwriteAsync(stewardRole, new OverwritePermissions(viewChannel: PermValue.Allow, muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow));
                await voiceChannel.AddPermissionOverwriteAsync(speakerRole, new OverwritePermissions(viewChannel: PermValue.Allow));
                await controlChannel.AddPermissionOverwriteAsync(attendeeRole, new OverwritePermissions(viewChannel: PermValue.Deny));
                await controlChannel.AddPermissionOverwriteAsync(speakerRole, new OverwritePermissions(viewChannel: PermValue.Deny));

                var eventId = Guid.NewGuid();
                var controlPanelBuilder = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x
                        .WithName($"Event {shortName}");
                    })
                    .WithColor(Colours.Primary)
                    .AddField("ID", eventId, true)
                    .AddField("Short ID", shortName, true)
                    .AddField("Host", Context.User.Mention, true);

                var eventPanelBuilder = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x
                        .WithIconUrl(Icons.Ticket)
                        .WithName(title);
                    })
                    .WithDescription(description)
                    .WithColor(Colours.Primary)
                    .AddField("Start", string.Format("{0:g}", start), true)
                    .AddField("Duration", durationFormatted, true)
                    .AddField("Organiser", Context.User.Mention, true)
                    .WithFooter($"{shortName} · {eventId}");

                if (speakers.Count() > 0)
                {
                    eventPanelBuilder.AddField($"Speakers ({speakers.Count()})", string.Join(" ", speakers.Select(x => x.Mention)));
                    controlPanelBuilder.AddField($"Speakers ({speakers.Count()})", string.Join(" ", speakers.Select(x => x.Mention)));
                }
                
                if (stewards.Count() > 0)
                    controlPanelBuilder.AddField($"Stewards ({stewards.Count()})", string.Join(" ", stewards.Select(x => x.Mention)));

                var eventPanelButtonsBuilder = new ComponentBuilder()
                    .WithButton("Sign up", "sign_up", ButtonStyle.Success);

                var controlPanelButtonsBuilder = new ComponentBuilder()
                    .WithButton("Start", "start", ButtonStyle.Success)
                    .WithButton("Stop", "stop", ButtonStyle.Danger)
                    .WithButton("Mass mute", "mass_mute", ButtonStyle.Danger)
                    .WithButton("Mass unmute", "mass_unmute", ButtonStyle.Danger);

                var eventsChannel = Context.Guild.GetTextChannel(Configuration.GetValue<ulong>("Events"));
                var eventPanel = await eventsChannel.SendMessageAsync(embed: eventPanelBuilder.Build(), component: eventPanelButtonsBuilder.Build());
                var controlPanel = await controlChannel.SendMessageAsync(embed: controlPanelBuilder.Build(), component: controlPanelButtonsBuilder.Build());
                var @event = new Event
                {
                    Id = eventId,
                    Title = title,
                    Description = description,
                    ShortName = shortName,
                    Guild = Context.Guild.Id,
                    Organiser = Context.User.Id,
                    Start = start,
                    Duration = duration,
                    Category = category.Id,
                    TextChannel = textChannel.Id,
                    VoiceChannel = voiceChannel.Id,
                    ControlChannel = controlChannel.Id,
                    ControlPanel = controlPanel.Id,
                    EventPanel = eventPanel.Id,
                    StewardRole = stewardRole.Id,
                    SpeakerRole = speakerRole.Id,
                    AttendeeRole = attendeeRole.Id,
                    CosmeticRole = cosmeticRoleId
                };

                await EventsDataAccessLayer.Create(@event);
            }
            catch (Exception ex)
            {
                await message.ModifyAsync(x => x.Content = "Whoops! It seems like something went wrong. Please try again later.");
                Log.Warning(ex.ToString());
                return;
            }

            await message.ModifyAsync(x => x.Content = $"Done! All roles and channels have been created for the event.");
        }
    }
}
