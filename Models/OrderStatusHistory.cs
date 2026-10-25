using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    // One row per status change. Used to show the order timeline.
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderStatus Status { get; set; }
        public DateTime ChangedAt { get; set; }

        // Who made the change (customer, merchant, driver or admin).
        public string? ChangedByUserId { get; set; }
        public User? ChangedByUser { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
