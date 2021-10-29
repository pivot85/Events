using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Events.Bot
{
    /// <summary>
    ///     Represents a service used to get Interactions.
    /// </summary>
    public class InteractionService
    {
        private static DiscordShardedClient _client;

        public static void Create(DiscordShardedClient client)
        {
            _client = client;
        }

        public static async Task<SocketMessageComponent> NextSelectMenuAsync(Predicate<SocketMessageComponent> filter, Func<SocketMessageComponent, Task> action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            filter ??= m => true;

            var cancelSource = new TaskCompletionSource<bool>();
            var componentSource = new TaskCompletionSource<SocketMessageComponent>();
            var cancellationRegistration = cancellationToken.Register(() => cancelSource.SetResult(true));

            var componentTask = componentSource.Task;
            var cancelTask = cancelSource.Task;
            var timeoutTask = timeout.HasValue ? Task.Delay(timeout.Value) : null;

            Task CheckComponent(SocketMessageComponent comp)
            {
                if (filter.Invoke(comp))
                {
                    componentSource.SetResult(comp);
                }
                else
                {
                    return action(comp);
                }

                return Task.CompletedTask;
            }

            Task HandleInteraction(SocketMessageComponent arg)
            {
                return CheckComponent(arg);
            }

            try
            {
                _client.SelectMenuExecuted += HandleInteraction;

                Task result;

                if (timeout.HasValue)
                    result = await Task.WhenAny(componentTask, cancelTask, timeoutTask).ConfigureAwait(false);
                else
                    result = await Task.WhenAny(componentTask, cancelTask).ConfigureAwait(false);

                return result == componentTask
                    ? await componentTask.ConfigureAwait(false)
                    : null;
            }
            finally
            {
                _client.SelectMenuExecuted -= HandleInteraction;
                cancellationRegistration.Dispose();
            }
        }

        /// <summary>
        ///     Retrieves the next incoming Message component interaction that passes the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">The <see cref="Predicate{SocketMessageComponent}"/> which the component has to pass.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
        /// <returns>The <see cref="SocketMessageComponent"/> that matches the provided filter.</returns>
        public static async Task<SocketMessageComponent> NextButtonAsync(Predicate<SocketMessageComponent> filter = null, Func<SocketMessageComponent, Task> action = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            filter ??= m => true;

            var cancelSource = new TaskCompletionSource<bool>();
            var componentSource = new TaskCompletionSource<SocketMessageComponent>();
            var cancellationRegistration = cancellationToken.Register(() => cancelSource.SetResult(true));

            var componentTask = componentSource.Task;
            var cancelTask = cancelSource.Task;
            var timeoutTask = timeout.HasValue ? Task.Delay(timeout.Value) : null;

            Task CheckComponent(SocketMessageComponent comp)
            {
                if (filter.Invoke(comp))
                {
                    componentSource.SetResult(comp);
                }
                else
                {
                    return action(comp);
                }

                return Task.CompletedTask;
            }

            Task HandleInteraction(SocketMessageComponent arg)
            {
                return CheckComponent(arg);
            }

            try
            {
                _client.ButtonExecuted += HandleInteraction;

                Task result;

                if(timeout.HasValue)
                    result = await Task.WhenAny(componentTask, cancelTask, timeoutTask).ConfigureAwait(false);
                else
                    result = await Task.WhenAny(componentTask, cancelTask).ConfigureAwait(false);

                return result == componentTask
                    ? await componentTask.ConfigureAwait(false)
                    : null;
            }
            finally
            {
                _client.ButtonExecuted -= HandleInteraction;
                cancellationRegistration.Dispose();
            }
        }

        /// <summary>
        /// Retrieves the next incoming Slash command interaction that passes the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">The <see cref="Predicate{SocketSlashCommand}"/> which the component has to pass.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
        /// <returns>The <see cref="SocketSlashCommand"/> that matches the provided filter.</returns>
        public static async Task<SocketSlashCommand> NextSlashCommandAsync(Predicate<SocketSlashCommand> filter = null, CancellationToken cancellationToken = default)
        {
            filter ??= m => true;

            var cancelSource = new TaskCompletionSource<bool>();
            var slashcommandSource = new TaskCompletionSource<SocketSlashCommand>();
            var cancellationRegistration = cancellationToken.Register(() => cancelSource.SetResult(true));

            var slashcommandTask = slashcommandSource.Task;
            var cancelTask = cancelSource.Task;

            Task CheckCommand(SocketSlashCommand comp)
            {
                if (filter.Invoke(comp))
                {
                    slashcommandSource.SetResult(comp);
                }

                return Task.CompletedTask;
            }

            Task HandleInteraction(SocketSlashCommand arg)
            {
                return CheckCommand(arg);
            }

            try
            {
                _client.SlashCommandExecuted += HandleInteraction;

                var result = await Task.WhenAny(slashcommandTask, cancelTask).ConfigureAwait(false);

                return result == slashcommandTask
                    ? await slashcommandTask.ConfigureAwait(false)
                    : null;
            }
            finally
            {
                _client.SlashCommandExecuted -= HandleInteraction;
                cancellationRegistration.Dispose();
            }
        }
    }
}
