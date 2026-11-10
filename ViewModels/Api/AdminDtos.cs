using System.ComponentModel.DataAnnotations;
using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    public class UserDto
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    // ?search=alice&role=Driver&page=1
    public class UserQuery : PageQuery
    {
        [MaxLength(100)]
        public string? Search { get; set; }

        [MaxLength(20)]
        public string? Role { get; set; }
    }

    public class SetUserActiveRequest
    {
        [Required]
        public bool? IsActive { get; set; }
    }

    public class ActivityLogDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string? UserEmail { get; set; }
        public ActivityType Action { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }

        public static ActivityLogDto FromEntity(ActivityLog log) => new ActivityLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserEmail = log.User?.Email,
            Action = log.Action,
            Details = log.Details,
            IpAddress = log.IpAddress,
            CreatedAt = log.CreatedAt
        };
    }

    public class ActivityQuery : PageQuery
    {
        [MaxLength(450)]
        public string? UserId { get; set; }
        public ActivityType? Action { get; set; }
    }

    // ?days=30 : the period the dashboard covers.
    public class AnalyticsQuery
    {
        [Range(1, 365)]
        public int Days { get; set; } = 30;
    }

    public class AnalyticsDto
    {
        public int Days { get; set; }

        // In the period
        public int TotalOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrRejectedOrders { get; set; }
        public decimal Revenue { get; set; }          // total of delivered orders
        public decimal DeliveryFees { get; set; }     // paid to drivers
        public decimal AverageOrderValue { get; set; }

        // Right now
        public int ActiveOrders { get; set; }
        public int ApprovedStores { get; set; }
        public int PendingStores { get; set; }
        public int OnlineDrivers { get; set; }
        public int TotalUsers { get; set; }
        public int PendingApplications { get; set; }

        public List<DailyStatDto> OrdersPerDay { get; set; } = new List<DailyStatDto>();
        public List<TopStoreDto> TopStores { get; set; } = new List<TopStoreDto>();
    }

    public class DailyStatDto
    {
        public DateTime Date { get; set; }
        public int Orders { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopStoreDto
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; } = "";
        public int DeliveredOrders { get; set; }
        public decimal Revenue { get; set; }
    }
}
