using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Events.Data.Context;

namespace Events.Bot
{
    class Program
    {
        private Logger _log;
        private IServiceProvider _services;

        static void Main(string[] args)
        {
            new Program().StartAsync().GetAwaiter().GetResult();
        }


        public async Task StartAsync()
        {
            ConfigService.LoadConfig();

            Logger.AddStream(Console.OpenStandardOutput(), StreamType.StandardOut);
            Logger.AddStream(Console.OpenStandardError(), StreamType.StandardError);
            Logger.AddStream(File.OpenWrite("./err.log"), StreamType.StandardError);

            _log = Logger.GetLogger<Program>();

            var services = new ServiceCollection();

            var commandService = new DualCommandService();

            commandService.Log += LogAsync;

            services.AddSingleton(commandService);

            services.AddSingleton<EventDbContext>();

            var client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged,
                MessageCacheSize = 50,
                AlwaysDownloadUsers = false,
                LogLevel = LogSeverity.Debug,
            });

            client.Log += LogAsync;

            services.AddSingleton(client);

            var handlerService = new HandlerService(client, services);
            services.AddSingleton(handlerService);

            var commandCoordinator = new ApplicationCommandCoordinator(client);
            services.AddSingleton(commandCoordinator);

            await client.LoginAsync(TokenType.Bot, ConfigService.Config.Token);
            await client.StartAsync();
            await client.SetStatusAsync(UserStatus.Idle);

            _log.Log("Services created <Green>successfully!</Green>");

            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            var msg = log.Message;

            if (log.Source.StartsWith("Audio ") && (msg?.StartsWith("Sent") ?? false))
                return Task.CompletedTask;

            Severity? sev = null;

            if (log.Source.StartsWith("Gateway"))
                sev = Severity.Socket;
            if (log.Source.StartsWith("Rest"))
                sev = Severity.Rest;

            _log.Write($"{log.Message}", sev.HasValue ? new Severity[] { sev.Value, log.Severity.ToLogSeverity() } : new Severity[] { log.Severity.ToLogSeverity() }, log.Exception);

            return Task.CompletedTask;
        }
    }
}
