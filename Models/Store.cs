using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    public enum StoreCategory
    {
        Restaurant,
        Grocery,
        Pharmacy,
        Other
    }

    // A store owned by a Merchant. Customers only see it after an Admin approves it.
    public class Store
    {
        public int Id { get; set; }

        [Required]
        public string OwnerId { get; set; } = "";
        public User Owner { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(1000)]
        public string? Description { get; set; }
        public StoreCategory Category { get; set; }
        public string? LogoUrl { get; set; }

        [Required, MaxLength(300)]
        public string Address { get; set; } = "";
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        [MaxLength(20)]
        public string? Phone { get; set; }

        public TimeOnly OpensAt { get; set; }
        public TimeOnly ClosesAt { get; set; }

        // Flat fee added to every order from this store (USD).
        public decimal DeliveryFee { get; set; }
        public decimal MinimumOrder { get; set; }

        public bool IsOpen { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
