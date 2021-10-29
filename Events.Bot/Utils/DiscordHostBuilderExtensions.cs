#region License
/*
   Copyright 2021 Hawxy
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 */
#endregion
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Bot.Utils
{
    public static class DiscordHostBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="CommandService"/> instance to the host for use with a Discord.Net client. />
        /// </summary>
        /// <remarks>
        /// A <see cref="HostBuilderContext"/> is supplied so that the configuration and service provider can be used.
        /// </remarks>
        /// <param name="builder">The host builder to configure.</param>
        /// <param name="config">The delegate for configuring the <see cref="CommandServiceConfig" /> that will be used to initialise the service.</param>
        /// <returns>The (generic) host builder.</returns>
        /// <exception cref="ArgumentNullException">Thrown if config is null</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="CommandService"/> is already added to the collection</exception>
        public static IHostBuilder UseDualCommandService(this IHostBuilder builder)
        {
            builder.ConfigureServices((context, collection) =>
            {
                if (collection.Any(x => x.ServiceType == typeof(DualCommandService)))
                    throw new InvalidOperationException("Cannot add more than one DualCommandService to host");

                collection.AddSingleton(x => new DualCommandService(
                    new CommandServiceConfig()
                    {
                        DefaultRunMode = RunMode.Async,
                        LogLevel = LogSeverity.Info
                    })
                );
                collection.AddHostedService<DualCommandServiceRegistrationHost>();
            });

            return builder;
        }
    }
}
