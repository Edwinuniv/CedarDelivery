using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    public enum RequestedRole
    {
        Merchant,
        Driver
    }

    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    // A customer's request to become a Merchant or a Driver. An Admin reviews it.
    // Same idea as CedarPay's AgentApplication.
    public class RoleApplication
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public User User { get; set; } = null!;

        public RequestedRole RequestedRole { get; set; }

        // Free text from the applicant (store name, vehicle, experience, ...).
        [MaxLength(2000)]
        public string? Details { get; set; }

        public ApplicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // The admin who approved or rejected it.
        public string? ReviewedByUserId { get; set; }
        public User? ReviewedByUser { get; set; }
        public DateTime? ReviewedAt { get; set; }
        [MaxLength(500)]
        public string? ReviewNote { get; set; }
    }
}
