using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public string? RedirectUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public static NotificationDto FromEntity(Notification notification) => new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            RedirectUrl = notification.RedirectUrl,
            CreatedAt = notification.CreatedAt
        };
    }
}
