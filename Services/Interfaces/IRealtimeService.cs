using CedarDelivery.Models;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // Pushes live updates to browsers through NotificationHub (SignalR).
    // Services call this; they never talk to the hub directly.
    public interface IRealtimeService
    {
        // To one user: a new notification (toast + badge).
        Task SendNotificationAsync(string userId, NotificationDto notification);

        // To everyone watching an order page: its status changed.
        Task OrderChangedAsync(int orderId, OrderStatus status);

        // To everyone watching an order page: where the driver is now.
        Task DriverLocationAsync(int orderId, decimal latitude, decimal longitude);
    }
}
