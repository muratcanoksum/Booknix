using Booknix.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Booknix.MVCUI.Hubs
{
    public class SignalRNotificationDispatcher : INotificationDispatcher
    {
        private readonly IHubContext<NotificationHub> _hub;

        public SignalRNotificationDispatcher(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task PushAsync(string key, object? payload = null)
        {
            await _hub.Clients.All.SendAsync(key, payload);
        }
    }
}
