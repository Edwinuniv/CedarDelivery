namespace CedarDelivery.Models
{
    public enum DeliveryStatus
    {
        Assigned,
        PickedUp,
        Delivered,
        Cancelled
    }

    // Created when a driver accepts an order (one delivery per order).
    public class Delivery
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int DriverProfileId { get; set; }
        public DriverProfile DriverProfile { get; set; } = null!;

        public DeliveryStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        // What the driver earns for this delivery (USD).
        // For now it equals the order's delivery fee; stored separately so the rule can change later.
        public decimal DriverEarning { get; set; }
    }
}
