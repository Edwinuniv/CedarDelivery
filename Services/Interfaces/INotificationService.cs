using CedarDelivery.Models;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    public interface INotificationService
    {
        // Sends an in-app notification (and an email copy when sendEmail is true).
        // Never throws: a failed notification must not undo the action that triggered it.
        Task NotifyAsync(string userId, string title, string message, NotificationType type, string? redirectUrl = null, bool sendEmail = false);

        Task<PagedResult<NotificationDto>> GetMineAsync(string userId, PageQuery query);
        Task<int> GetUnreadCountAsync(string userId);
        // Returns the notification so the caller can open its RedirectUrl.
        Task<NotificationDto> MarkReadAsync(int id, string userId);
        Task MarkAllReadAsync(string userId);
    }
}
