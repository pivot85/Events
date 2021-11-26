using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Events.Bot.Common;
using Events.Bot.Common.RequestResult;
using Events.Bot.Extensions;
using Events.Data.DataAccessLayer;
using Events.Data.Models;
using Interactivity;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TimeZoneNames;

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
                await Context.Interaction.RespondAsync("You are not allowed to create new events. Only administrators and permitted roles can.");
                return;
            }

            if (Context.Guild.Channels.Count() > (GUILD_CHANNELS_CAP - REQUIRED_AVAILABLE_CHANNELS_COUNT) ||
                Context.Guild.Roles.Count() > (GUILD_ROLES_CAP - REQUIRED_AVAILABLE_ROLES_COUNT))
            {
                await Context.Interaction.RespondAsync("This server is nearing the maximum amount of roles or channels, thus a new event cannot be created.");
                return;
            }

            await Context.Interaction.RespondAsync("Let's create a new event! If you want to cancel at any time, simply respond with \"cancel\".");

            var titleResult = await Ask<string>(
                $"First, what would you like the title to be? The title must be shorter than {TITLE_CHAR_LIMIT} characters.",
                max: TITLE_CHAR_LIMIT,
                minMaxError: $"Please provide a title that is shorter than or equal to {TITLE_CHAR_LIMIT} characters.");
            if (titleResult.Type == RequestResultType.Cancelled)
                return;

            var descriptionResult = await Ask<string>(
                $"Title set! What description would you like the event to have? It must be shorter than {DESC_CHAR_LIMIT} characters.",
                max: DESC_CHAR_LIMIT,
                minMaxError: $"Please pick a description that is shorter than {DESC_CHAR_LIMIT} characters.");
            if (descriptionResult.Type == RequestResultType.Cancelled)
                return;

            var shortNameResult = await Ask<string>(
                $"Description set! What would you like the short name of this event to be? " +
                $"This will appear in the channels. This can contain a maximum of {SHORT_NAME_CHAR_LIMIT} characters.",
                max: SHORT_NAME_CHAR_LIMIT,
                minMaxError: $"Please provide a short name containing less than or equal to {SHORT_NAME_CHAR_LIMIT} characters.",
                criterion: RequestCriterion.ShortName);
            if (shortNameResult.Type == RequestResultType.Cancelled)
                return;

            var timeZones = TZNames.GetAbbreviationsForTimeZone(TimeZoneInfo.Local.Id, "en-US");
            var timeZone = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? timeZones.Daylight : timeZones.Standard;

            var startResult = await Ask<DateTime>(
                $"All set. When should the event take place? Please format it as `mm/dd/yyyy HH:MM:SS` ({timeZone}).",
                minMaxError: "Please provide a properly formatted date and time for the start of the event.");
            if (startResult.Type == RequestResultType.Cancelled)
                return;

            var durationResult = await Ask<TimeSpan>(
                $"The event will start on {string.Format("{0:g}", startResult.Value)}. How long will the event last? Please provide the duration as HH:MM:SS.",
                10,
                24,
                "Please provide a duration that is between 10 minutes and 24 hours.",
                parseFailedMessage: "Please provide a duration formatted as HH:MM:SS.");
            if (durationResult.Type == RequestResultType.Cancelled)
                return;

            var stewardRole = await Context.Guild.CreateRoleAsync(shortNameResult.Value + " Steward", isHoisted: false, isMentionable: false);
            var stewardsResult = await Ask<List<RestGuildUser>>(
                $"The event will last for {durationResult.Value.ToHoursMinutes()}. Do you want to add any stewards? Please mention them or respond with \"skip\".");

            if (stewardsResult.Type == RequestResultType.Cancelled)
            {
                await stewardRole.DeleteAsync();
                return;
            }

            if (stewardsResult.Type == RequestResultType.Success)
            {
                foreach (var user in stewardsResult.Value)
                {
                    await user.AddRoleAsync(stewardRole);
                }
            }

            var speakerRole = await Context.Guild.CreateRoleAsync(shortNameResult.Value + " Speaker", isHoisted: false, isMentionable: false);
            var speakersResult = await Ask<List<RestGuildUser>>(
                $"Stewards have been set! Do you want to add any speakers? Please mention them or respond with \"skip\".");

            if (speakersResult.Type == RequestResultType.Cancelled)
            {
                await speakerRole.DeleteAsync();
                return;
            }

            if (speakersResult.Type == RequestResultType.Success)
            {
                foreach (var user in speakersResult.Value)
                {
                    await user.AddRoleAsync(speakerRole);
                }
            }

            ulong cosmeticRoleId = 0;
            if ((Context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                var cosmeticRoleResult = await Ask<SocketRole>(
                    $"Is there a role you'd like to use as a cosmetic role? Please mention it or respond with \"skip\".");

                if (cosmeticRoleResult.Type == RequestResultType.Cancelled)
                    return;

                if (cosmeticRoleResult.Type == RequestResultType.Success)
                    cosmeticRoleId = cosmeticRoleResult.Value.Id;
            }

            var message = await Context.Channel.SendMessageAsync("That's it! Let's create your event...");
            try
            {
                var attendeeRole = await Context.Guild.CreateRoleAsync(shortNameResult.Value + " Attendee", isHoisted: false, isMentionable: false);
                var hostRole = await Context.Guild.CreateRoleAsync(shortNameResult.Value + " Host", isHoisted: false, isMentionable: false);

                // Permission overwrites for the category of the event.
                var categoryOverwrites = new List<Overwrite>()
                {
                    new Overwrite(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(sendMessages: PermValue.Deny)),
                    new Overwrite(hostRole.Id, PermissionTarget.Role, new OverwritePermissions(sendMessages: PermValue.Allow)),
                    new Overwrite(stewardRole.Id, PermissionTarget.Role, new OverwritePermissions(sendMessages: PermValue.Allow)),
                };

                // Creates a category channel and adds the text, voice and control channel of the event to it.
                var category = await Context.Guild.CreateCategoryChannelAsync("Event " + shortNameResult.Value, x => x.PermissionOverwrites = categoryOverwrites);
                var textChannel = await Context.Guild.CreateTextChannelAsync(shortNameResult.Value + "-general", x => x.CategoryId = category.Id);
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync("Event " + shortNameResult.Value, x => x.CategoryId = category.Id);
                var controlChannel = await Context.Guild.CreateTextChannelAsync(shortNameResult.Value + "-control", x => x.CategoryId = category.Id);

                // Permission overwrites for the voice channel of the event.
                // Attendees can't connect, the host and stewards receive permissions to mute, deafen and move members.
                // The speaker can connect but doesn't receive additional permissions.
                await voiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, speak: PermValue.Deny, stream: PermValue.Deny));
                await voiceChannel.AddPermissionOverwriteAsync(hostRole, new OverwritePermissions(muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow));
                await voiceChannel.AddPermissionOverwriteAsync(stewardRole, new OverwritePermissions(muteMembers: PermValue.Allow, deafenMembers: PermValue.Allow, moveMembers: PermValue.Allow));

                // Permissions overwrites for the control channel of the event. Only the host and stewards can see this channel.
                await controlChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
                await controlChannel.AddPermissionOverwriteAsync(stewardRole, new OverwritePermissions(viewChannel: PermValue.Allow));
                await controlChannel.AddPermissionOverwriteAsync(hostRole, new OverwritePermissions(viewChannel: PermValue.Allow));

                // Generates a random GUID for the event.
                var eventId = Guid.NewGuid();

                // The embed for the control panel, shown to the host and stewards.
                var controlPanelBuilder = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x
                        .WithName($"Event {shortNameResult.Value}");
                    })
                    .WithColor(Colours.Primary)
                    .AddField("Title", titleResult.Value, true)
                    .AddField("Short name", shortNameResult.Value, true)
                    .AddField("Host", Context.User.Mention, true);

                if (speakersResult.Type == RequestResultType.Success)
                    controlPanelBuilder.AddField($"Speakers ({speakersResult.Value.Count()})", string.Join(" ", speakersResult.Value.Select(x => x.Mention)));

                if (stewardsResult.Type == RequestResultType.Success)
                    controlPanelBuilder.AddField($"Stewards ({stewardsResult.Value.Count()})", string.Join(" ", stewardsResult.Value.Select(x => x.Mention)));

                var controlPanelButtonsBuilder = new ComponentBuilder()
                    .WithButton("Start", "start", ButtonStyle.Success, row: 0)
                    .WithButton("Stop", "stop", ButtonStyle.Danger, row: 0)
                    .WithButton("Lock", "lock", ButtonStyle.Secondary, row: 1)
                    .WithButton("Unlock", "unlock", ButtonStyle.Secondary, row: 1)
                    .WithButton("Mass mute", "mass_mute", ButtonStyle.Secondary, row: 2)
                    .WithButton("Mass unmute", "mass_unmute", ButtonStyle.Secondary, row: 2);

                var eventsChannel = Context.Guild.GetTextChannel(Configuration.GetValue<ulong>("Events"));
                var guildEvent = await Context.Guild.CreateEventAsync(titleResult.Value,
                    startResult.Value,
                    GuildScheduledEventType.Voice,
                    GuildScheduledEventPrivacyLevel.Private,
                    descriptionResult.Value + $"\n\n" +
                    $"This event is hosted by {Context.User.Mention}. " +
                    (speakersResult.Type == RequestResultType.Success ? $"This event will have {speakersResult.Value.Count()} speaker(s): {string.Join(", ", speakersResult.Value.Select(x => x.Mention))}. " : "") +
                    $"The expected duration is {durationResult.Value.ToHoursMinutes()}." +
                    (cosmeticRoleId > 0 ? $" This event has a special role that you'll receive when attending: <@&{cosmeticRoleId}>." : ""),
                    channelId: voiceChannel.Id);

                await eventsChannel.SendMessageAsync($"A new event **{titleResult.Value}** was just created by {Context.User.Mention}!\n\n{await guildEvent.GetUrlAsync(voiceChannel.Id)}");
                var controlPanel = await controlChannel.SendMessageAsync(embed: controlPanelBuilder.Build(), component: controlPanelButtonsBuilder.Build());
                var @event = new Event
                {
                    Id = guildEvent.Id,
                    Title = titleResult.Value,
                    Description = descriptionResult.Value,
                    ShortName = shortNameResult.Value,
                    Guild = Context.Guild.Id,
                    Organiser = Context.User.Id,
                    Start = startResult.Value,
                    Duration = durationResult.Value,
                    Category = category.Id,
                    TextChannel = textChannel.Id,
                    VoiceChannel = voiceChannel.Id,
                    ControlChannel = controlChannel.Id,
                    ControlPanel = controlPanel.Id,
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
            var deleteResult = await Ask<string>(
                "Do you want to delete all the messages related to this setup (yes/no)?",
                acceptedResponses: new string[] { "yes", "no" });

            if (deleteResult.Type != RequestResultType.Success)
                return;

            var originalResponse = await Context.Interaction.GetOriginalResponseAsync();
            var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
            messages = messages
                .Where(x => x.CreatedAt >= originalResponse.CreatedAt &&
                (x.Author.Id == Context.User.Id || x.Author.Id == Context.Client.CurrentUser.Id));

            if (deleteResult.Value.ToLower() == "no")
            {
                await messages.First(x => x.Content.ToLower() == "no").AddReactionAsync(new Emoji("✅"));
                return;
            }

            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
            var deleteMessage = await Context.Channel.SendMessageAsync("Done! This messages will delete in 5 seconds...");
            await Task.Delay(5000);
            await deleteMessage.DeleteAsync();
        }
    }
}
