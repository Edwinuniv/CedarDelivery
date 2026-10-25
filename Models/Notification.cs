using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    public enum NotificationType
    {
        General,
        NewOrder,
        OrderStatusChanged,
        DeliveryAvailable,
        ApplicationApproved,
        ApplicationRejected,
        NewReview
    }

    // In-app notification shown in the bell menu.
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public User User { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Title { get; set; } = "";
        [Required, MaxLength(500)]
        public string Message { get; set; } = "";
        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        // Page to open when the notification is clicked, e.g. "/Order/Details/5".
        public string? RedirectUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
