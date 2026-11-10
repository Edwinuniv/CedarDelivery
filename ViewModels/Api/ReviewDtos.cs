using System.ComponentModel.DataAnnotations;
using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public ReviewTarget TargetType { get; set; }
        public int? StoreId { get; set; }
        public int? DriverProfileId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        // "Alice H." — first name and initial only, for privacy on public pages.
        public string CustomerName { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        // Needs Customer loaded.
        public static ReviewDto FromEntity(Review review) => new ReviewDto
        {
            Id = review.Id,
            OrderId = review.OrderId,
            TargetType = review.TargetType,
            StoreId = review.StoreId,
            DriverProfileId = review.DriverProfileId,
            Rating = review.Rating,
            Comment = review.Comment,
            CustomerName = review.Customer == null
                ? ""
                : $"{review.Customer.FirstName} {(string.IsNullOrEmpty(review.Customer.LastName) ? "" : review.Customer.LastName[0] + ".")}".Trim(),
            CreatedAt = review.CreatedAt
        };
    }

    public class CreateReviewRequest
    {
        [Required]
        public int? OrderId { get; set; }

        [Required]
        public ReviewTarget? TargetType { get; set; }

        [Required, Range(1, 5)]
        public int? Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    // A page of reviews plus the overall score.
    public class ReviewListDto
    {
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public PagedResult<ReviewDto> Reviews { get; set; } = new PagedResult<ReviewDto>();
    }
}
