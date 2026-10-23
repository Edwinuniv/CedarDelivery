using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    // Application user. Extends IdentityUser (string Id, Email, PasswordHash, ...).
    // Phase 1 adds the delivery-specific fields and navigation properties.
    public class User : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = "";
        [Required]
        public string LastName { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
