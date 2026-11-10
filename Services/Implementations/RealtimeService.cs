using Microsoft.AspNetCore.SignalR;
using CedarDelivery.Hubs;
using CedarDelivery.Models;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class RealtimeService : IRealtimeService
    {
        // IHubContext lets code outside the hub send messages to connected browsers.
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<RealtimeService> _logger;

        public RealtimeService(IHubContext<NotificationHub> hub, ILogger<RealtimeService> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public Task SendNotificationAsync(string userId, NotificationDto notification) =>
            SafeSendAsync(() => _hub.Clients.User(userId).SendAsync("notification", notification));

        public Task OrderChangedAsync(int orderId, OrderStatus status) =>
            SafeSendAsync(() => _hub.Clients.Group(NotificationHub.OrderGroup(orderId))
                .SendAsync("orderUpdated", new { orderId, status }));

        public Task DriverLocationAsync(int orderId, decimal latitude, decimal longitude) =>
            SafeSendAsync(() => _hub.Clients.Group(NotificationHub.OrderGroup(orderId))
                .SendAsync("driverLocation", new { orderId, latitude, longitude }));

        // Live updates are a bonus: the data is already saved, and pages also
        // refresh on their own. A failed push must never fail the user's action.
        private async Task SafeSendAsync(Func<Task> send)
        {
            try
            {
                await send();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not send a real-time update.");
            }
        }
    }
}
