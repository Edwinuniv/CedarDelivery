using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    public interface IReviewService
    {
        // Customer rates the store or the driver of one of their delivered orders.
        Task<ReviewDto> CreateAsync(string userId, CreateReviewRequest request);
        Task<List<ReviewDto>> GetMineAsync(string userId);

        // Public store reviews; a driver's own reviews.
        Task<ReviewListDto> GetStoreReviewsAsync(int storeId, PageQuery query);
        Task<ReviewListDto> GetDriverReviewsAsync(string driverUserId, PageQuery query);
    }
}
