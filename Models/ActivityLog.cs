using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    public enum ActivityType
    {
        Login,
        Logout,
        Register,
        ProfileUpdated,
        PasswordChanged,
        AddressAdded,
        AddressRemoved,
        OrderPlaced,
        OrderCancelled,
        OrderStatusChanged,
        DeliveryAccepted,
        DeliveryCompleted,
        StoreUpdated,
        ProductChanged,
        ReviewPosted,
        ApplicationSubmitted,
        ApplicationReviewed
    }

    // A record of something a user did, for security and auditing (same as CedarPay).
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public User User { get; set; } = null!;

        public ActivityType Action { get; set; }

        [MaxLength(500)]
        public string? Details { get; set; }
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
