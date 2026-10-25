using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    // Application user. Extends IdentityUser (string Id, Email, PhoneNumber, PasswordHash, ...).
    // The same User can be a Customer, a Merchant (owns stores) or a Driver (has a DriverProfile).
    public class User : IdentityUser
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = "";
        [Required, MaxLength(50)]
        public string LastName { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Store> Stores { get; set; } = new List<Store>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<RoleApplication> RoleApplications { get; set; } = new List<RoleApplication>();
        public DriverProfile? DriverProfile { get; set; }
    }
}
