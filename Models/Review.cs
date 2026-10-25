using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    public enum ReviewTarget
    {
        Store,
        Driver
    }

    // A rating left by a customer after an order is delivered.
    // TargetType says what is rated: StoreId is set for store reviews,
    // DriverProfileId for driver reviews. One of each per order at most.
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = "";
        public User Customer { get; set; } = null!;

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public ReviewTarget TargetType { get; set; }

        public int? StoreId { get; set; }
        public Store? Store { get; set; }

        public int? DriverProfileId { get; set; }
        public DriverProfile? DriverProfile { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }
        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
