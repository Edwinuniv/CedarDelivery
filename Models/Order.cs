using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    // Normal flow: Pending -> Accepted -> Preparing -> ReadyForPickup -> PickedUp -> Delivered.
    // Rejected = the store refused it. Cancelled = the customer cancelled while Pending.
    public enum OrderStatus
    {
        Pending,
        Accepted,
        Preparing,
        ReadyForPickup,
        PickedUp,
        Delivered,
        Rejected,
        Cancelled
    }

    public enum PaymentMethod
    {
        CashOnDelivery,
        Card
    }

    public enum PaymentStatus
    {
        Unpaid,
        Paid,
        Refunded
    }

    public class Order
    {
        public int Id { get; set; }

        // Human-friendly number shown to users, e.g. "CD-20261025-0001".
        [Required, MaxLength(30)]
        public string OrderNumber { get; set; } = "";

        [Required]
        public string CustomerId { get; set; } = "";
        public User Customer { get; set; } = null!;

        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        public OrderStatus Status { get; set; }

        // Prices in USD. Total = Subtotal + DeliveryFee.
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // A copy of the address at order time, so editing or deleting
        // a saved Address never changes where a past order went.
        [Required, MaxLength(500)]
        public string DeliveryAddress { get; set; } = "";
        public decimal? DeliveryLatitude { get; set; }
        public decimal? DeliveryLongitude { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public Delivery? Delivery { get; set; }
    }
}
