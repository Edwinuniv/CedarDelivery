using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    // A saved delivery address of a customer ("Home", "Work", ...).
    public class Address
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public User User { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Label { get; set; } = "";
        [Required, MaxLength(100)]
        public string City { get; set; } = "";
        [Required, MaxLength(200)]
        public string Street { get; set; } = "";
        [MaxLength(100)]
        public string? Building { get; set; }
        [MaxLength(20)]
        public string? Floor { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Map pin. Optional, because the customer may not pick one.
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
