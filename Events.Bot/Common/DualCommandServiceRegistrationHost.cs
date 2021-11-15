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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Bot.Common
{
    internal class DualCommandServiceRegistrationHost : IHostedService
    {
        private readonly DualCommandService _dualCommandService;
        private readonly ILogger<DualCommandServiceRegistrationHost> _logger;
        private readonly LogAdapter<DualCommandService> _adapter;

        public DualCommandServiceRegistrationHost(DualCommandService commandService, ILogger<DualCommandServiceRegistrationHost> logger, LogAdapter<DualCommandService> adapter)
        {
            _dualCommandService = commandService;
            _logger = logger;
            _adapter = adapter;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _dualCommandService.Log += _adapter.Log;
            _logger.LogDebug("Registered logger for CommandService");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
