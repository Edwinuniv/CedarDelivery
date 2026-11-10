using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IDriverProfileRepository _driverProfileRepository;
        private readonly INotificationService _notifications;
        private readonly IActivityLogService _activityLog;

        public ReviewService(
            IReviewRepository reviewRepository,
            IOrderRepository orderRepository,
            IStoreRepository storeRepository,
            IDriverProfileRepository driverProfileRepository,
            INotificationService notifications,
            IActivityLogService activityLog)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
            _storeRepository = storeRepository;
            _driverProfileRepository = driverProfileRepository;
            _notifications = notifications;
            _activityLog = activityLog;
        }

        public async Task<ReviewDto> CreateAsync(string userId, CreateReviewRequest request)
        {
            var order = await _orderRepository.GetWithDetailsAsync(request.OrderId!.Value);

            if (order == null || order.CustomerId != userId)
            {
                throw new NotFoundException("Order not found.");
            }

            if (order.Status != OrderStatus.Delivered)
            {
                throw new BadRequestException("You can review an order after it is delivered.");
            }

            var target = request.TargetType!.Value;

            if (target == ReviewTarget.Driver && order.Delivery == null)
            {
                throw new BadRequestException("This order has no driver to review.");
            }

            if (await _reviewRepository.ExistsAsync(order.Id, target))
            {
                throw new BadRequestException($"You already reviewed the {target.ToString().ToLower()} for this order.");
            }

            var review = new Review
            {
                CustomerId = userId,
                Customer = order.Customer,
                OrderId = order.Id,
                TargetType = target,
                StoreId = target == ReviewTarget.Store ? order.StoreId : null,
                DriverProfileId = target == ReviewTarget.Driver ? order.Delivery!.DriverProfileId : null,
                Rating = request.Rating!.Value,
                Comment = request.Comment?.Trim(),
                CreatedAt = DateTime.Now
            };

            await _reviewRepository.AddAsync(review);

            // Let the store owner or the driver know.
            var recipientId = target == ReviewTarget.Store ? order.Store.OwnerId : order.Delivery!.DriverProfile.UserId;
            await _notifications.NotifyAsync(recipientId,
                $"New {review.Rating}★ review",
                string.IsNullOrWhiteSpace(review.Comment)
                    ? $"Order {order.OrderNumber} was rated {review.Rating}/5."
                    : $"Order {order.OrderNumber}: \"{review.Comment}\"",
                NotificationType.NewReview);
            await _activityLog.LogAsync(userId, ActivityType.ReviewPosted, $"Rated the {target.ToString().ToLower()} of order {order.OrderNumber} {review.Rating}/5.");

            return ReviewDto.FromEntity(review);
        }

        public async Task<List<ReviewDto>> GetMineAsync(string userId)
        {
            var reviews = await _reviewRepository.GetByCustomerAsync(userId);
            return reviews.Select(ReviewDto.FromEntity).ToList();
        }

        public async Task<ReviewListDto> GetStoreReviewsAsync(int storeId, PageQuery query)
        {
            var store = await _storeRepository.GetByIdAsync(storeId);
            if (store == null || !store.IsApproved)
            {
                throw new NotFoundException("Store not found.");
            }

            var (items, totalCount) = await _reviewRepository.GetForStorePagedAsync(storeId, query.Page, query.PageSize);
            var ratings = await _reviewRepository.GetStoreRatingsAsync(new[] { storeId });
            ratings.TryGetValue(storeId, out var rating);

            return new ReviewListDto
            {
                AverageRating = rating.Count == 0 ? null : rating.Average,
                ReviewCount = rating.Count,
                Reviews = ToPaged(items, totalCount, query)
            };
        }

        public async Task<ReviewListDto> GetDriverReviewsAsync(string driverUserId, PageQuery query)
        {
            var profile = await _driverProfileRepository.GetByUserIdAsync(driverUserId)
                ?? throw new NotFoundException("You don't have a driver profile yet.");

            var (items, totalCount) = await _reviewRepository.GetForDriverPagedAsync(profile.Id, query.Page, query.PageSize);
            var (average, count) = await _reviewRepository.GetDriverRatingAsync(profile.Id);

            return new ReviewListDto
            {
                AverageRating = average,
                ReviewCount = count,
                Reviews = ToPaged(items, totalCount, query)
            };
        }

        private static PagedResult<ReviewDto> ToPaged(List<Review> items, int totalCount, PageQuery query)
        {
            return new PagedResult<ReviewDto>
            {
                Items = items.Select(ReviewDto.FromEntity).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
