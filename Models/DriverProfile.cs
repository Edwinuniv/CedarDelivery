using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    public enum VehicleType
    {
        Motorcycle,
        Car,
        Bicycle
    }

    // Extra information for users who are drivers (one profile per user).
    public class DriverProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public User User { get; set; } = null!;

        public VehicleType VehicleType { get; set; }
        [MaxLength(20)]
        public string? PlateNumber { get; set; }

        // Only online drivers see new deliveries.
        public bool IsOnline { get; set; }

        // Last known position, for the live map (Extra feature).
        public decimal? CurrentLatitude { get; set; }
        public decimal? CurrentLongitude { get; set; }
        public DateTime? LocationUpdatedAt { get; set; }

        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
