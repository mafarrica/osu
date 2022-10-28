// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API;

namespace osu.Game.Online.Notifications.Polling
{
    /// <summary>
    /// A connector for <see cref="PollingNotificationsClient"/>s that poll for new messages.
    /// </summary>
    public class PollingNotificationsClientConnector : NotificationsClientConnector
    {
        private readonly IAPIProvider api;

        public PollingNotificationsClientConnector(IAPIProvider api)
            : base(api)
        {
            this.api = api;
        }

        protected override Task<NotificationsClient> BuildNotificationClientAsync(CancellationToken cancellationToken)
            => Task.FromResult((NotificationsClient)new PollingNotificationsClient(api));
    }
}
