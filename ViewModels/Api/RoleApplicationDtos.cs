using System.ComponentModel.DataAnnotations;
using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    public class RoleApplicationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string? UserEmail { get; set; }
        public RequestedRole RequestedRole { get; set; }
        public string? Details { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }

        // Needs User loaded.
        public static RoleApplicationDto FromEntity(RoleApplication application) => new RoleApplicationDto
        {
            Id = application.Id,
            UserId = application.UserId,
            UserName = application.User == null ? "" : $"{application.User.FirstName} {application.User.LastName}",
            UserEmail = application.User?.Email,
            RequestedRole = application.RequestedRole,
            Details = application.Details,
            Status = application.Status,
            CreatedAt = application.CreatedAt,
            ReviewedAt = application.ReviewedAt,
            ReviewNote = application.ReviewNote
        };
    }

    // Apply to become a Merchant or a Driver. Drivers must also give their vehicle.
    public class ApplyForRoleRequest
    {
        [Required]
        public RequestedRole? RequestedRole { get; set; }

        [Required, MaxLength(2000)]
        public string Details { get; set; } = "";

        public VehicleType? VehicleType { get; set; }

        [MaxLength(20)]
        public string? PlateNumber { get; set; }
    }

    public class ReviewApplicationRequest
    {
        [Required]
        public bool? Approve { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class ApplicationQuery : PageQuery
    {
        public ApplicationStatus? Status { get; set; }
    }
}
