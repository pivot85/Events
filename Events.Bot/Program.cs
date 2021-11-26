using System;
using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Events.Bot.Handlers;
using Events.Bot.Common;
using Events.Data.Context;
using Events.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Events.Data.DataAccessLayer;
using Interactivity;
using Events.Bot.Extensions;

namespace Events.Bot
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Hour)
                .CreateLogger();

            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Info,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200
                    };

                    config.Token = context.Configuration.GetValue<string>("Token");
                    config.LogFormat = (message, exception) => $"{message.Source}: {message.Message}";
                })
                .UseDualCommandService()
                .ConfigureServices((context, services) =>
                {
                    services
                    .AddSingleton<LogAdapter<DualCommandService>>()
                    .AddDbContextFactory<EventsDbContext>(options =>
                            options
                            .UseMySql(
                                context.Configuration.GetConnectionString("Default"),
                                new MySqlServerVersion(new Version(8, 0, 26))
                                ))
                    .AddSingleton<EventsDataAccessLayer>()
                    .AddSingleton<InteractivityService>()
                    .AddSingleton(x => new InteractivityConfig
                    { 
                        DefaultTimeout = TimeSpan.FromMinutes(5)
                    })
                    .AddSingleton<PermittedRolesDataAccessLayer>()
                    .AddHostedService<CommandHandler>()
                    .AddHostedService<ScheduledEventHandler>()
                    .AddHostedService<ApplicationCommandCoordinator>();
                });
    }
}
