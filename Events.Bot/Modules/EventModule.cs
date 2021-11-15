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

            await Context.Guild.DeleteApplicationCommandsAsync();

            await Context.Channel.SendMessageAsync("Clean-up finished!");
        }

        [Command("new")]
        public async Task NewEventAsync()
        {
            await Context.Interaction.DeferAsync();

            var permittedRoles = await PermittedRoleDataAccessLayer.GetAllByGuild(Context.Guild.Id);
            if (!await UserIsPermitted())
            {
                await Context.Interaction.FollowupAsync("You are not allowed to create new events. Only administrators and permitted roles can.");
                return;
            }

            if (Context.Guild.Channels.Count() > (GUILD_CHANNELS_CAP - REQUIRED_AVAILABLE_CHANNELS_COUNT) ||
                Context.Guild.Roles.Count() > (GUILD_ROLES_CAP - REQUIRED_AVAILABLE_ROLES_COUNT))
            {
                await Context.Interaction.FollowupAsync("This server is nearing the maximum amount of roles or channels, thus a new event cannot be created.");
                return;
            }

            var titleResult = await Ask<string>(
                $"Let's create a new event. If you want to cancel at any time, you can respond with \"cancel\". " +
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

            var startResult = await Ask<DateTime>(
                "All set. When should the event take place? Please format it as mm/dd/yyyy HH:MM:SS",
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

            var message = await Context.Channel.SendMessageAsync("All set! Time to create the event...");
            try
            {
                var attendeeRole = await Context.Guild.CreateRoleAsync(shortNameResult.Value + " Attendee", isHoisted: false, isMentionable: false);
                var hostRole = await Context.Guild.CreateRoleAsync(shortNameResult.Value + " Host", isHoisted: false, isMentionable: false);

                var categoryOverwrites = new List<Overwrite>();
                categoryOverwrites.Add(new Overwrite(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)));
                categoryOverwrites.Add(new Overwrite(hostRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)));
                categoryOverwrites.Add(new Overwrite(stewardRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)));
                categoryOverwrites.Add(new Overwrite(speakerRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)));
                categoryOverwrites.Add(new Overwrite(attendeeRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)));

                var category = await Context.Guild.CreateCategoryChannelAsync("Event " + shortNameResult.Value, x => x.PermissionOverwrites = categoryOverwrites);
                var textChannel = await Context.Guild.CreateTextChannelAsync(shortNameResult.Value + "-general", x => x.CategoryId = category.Id);
                var voiceChannel = await Context.Guild.CreateVoiceChannelAsync("Event " + shortNameResult.Value, x => x.CategoryId = category.Id);
                var controlChannel = await Context.Guild.CreateTextChannelAsync(shortNameResult.Value + "-control", x => x.CategoryId = category.Id);

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
                        .WithName($"Event {shortNameResult.Value}");
                    })
                    .WithColor(Colours.Primary)
                    .AddField("ID", eventId, true)
                    .AddField("Short ID", shortNameResult.Value, true)
                    .AddField("Host", Context.User.Mention, true);

                var eventPanelBuilder = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x
                        .WithIconUrl(Icons.Ticket)
                        .WithName(titleResult.Value);
                    })
                    .WithDescription(descriptionResult.Value)
                    .WithColor(Colours.Primary)
                    .AddField("Start", string.Format("{0:g}", startResult.Value), true)
                    .AddField("Duration", durationFormatted, true)
                    .AddField("Organiser", Context.User.Mention, true)
                    .WithFooter($"{shortNameResult.Value} · {eventId}");

                if (speakersResult.Value.Count() > 0)
                {
                    eventPanelBuilder.AddField($"Speakers ({speakersResult.Value.Count()})", string.Join(" ", speakersResult.Value.Select(x => x.Mention)));
                    controlPanelBuilder.AddField($"Speakers ({speakersResult.Value.Count()})", string.Join(" ", speakersResult.Value.Select(x => x.Mention)));
                }
                
                if (stewardsResult.Value.Count() > 0)
                    controlPanelBuilder.AddField($"Stewards ({stewardsResult.Value.Count()})", string.Join(" ", stewardsResult.Value.Select(x => x.Mention)));

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
