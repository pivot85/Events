using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Bot.Handlers
{
    public class CommandHandler : DiscordClientService
    {
        private IServiceProvider _services;
        private DualCommandService _commandService;

        public CommandHandler(DiscordSocketClient client, IServiceProvider provider, ILogger<DiscordClientService> logger)
            : base(client, logger)
        {
            _services = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client.SlashCommandExecuted += _client_SlashCommandExecuted;
            Client.MessageReceived += _client_MessageReceived;

            _commandService = _services.GetRequiredService<DualCommandService>();

            await _commandService.RegisterModulesAsync(Assembly.GetExecutingAssembly(), _services).ConfigureAwait(false);
        }

        private Task _client_MessageReceived(SocketMessage arg)
        {
            // TODO: Implement prefix check and execute command.

            return Task.CompletedTask;
        }

        private async Task _client_SlashCommandExecuted(SocketSlashCommand arg)
        {
            var context = new DualCommandContext(Client, arg);

            await _commandService.ExecuteAsync(arg, context, _services).ConfigureAwait(false);
        }
    }
}
