using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.ViewModels.Api
{
    // What a user can change about themselves. The phone number matters:
    // drivers and stores call it when they can't find the address.
    public class ProfileRequest
    {
        [Required, MaxLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = "";

        [Phone, MaxLength(20)]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }
}
