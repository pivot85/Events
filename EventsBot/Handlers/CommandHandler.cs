using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot.Handlers
{
    public class CommandHandler : DiscordHandler
    {
        private DiscordSocketClient _client;
        private IServiceProvider _services;
        private DualCommandService _commandService;

        public override async Task InitializeAsync(DiscordSocketClient client, IServiceProvider provider)
        {
            _client = client;
            _services = provider;

            _client.SlashCommandExecuted += _client_SlashCommandExecuted;
            _client.MessageReceived += _client_MessageReceived;

            _commandService = provider.GetRequiredService<DualCommandService>();

            await _commandService.RegisterModulesAsync(Assembly.GetExecutingAssembly(), provider).ConfigureAwait(false);


        }

        private Task _client_MessageReceived(SocketMessage arg)
        {
            // TODO: Implement prefix check and execute command.

            return Task.CompletedTask;
        }

        private async Task _client_SlashCommandExecuted(SocketSlashCommand arg)
        {
            var context = new DualCommandContext(_client, arg);

            await _commandService.ExecuteAsync(arg, context, _services).ConfigureAwait(false);
        }
    }
}
