using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<bool> ExistsAsync(int orderId, ReviewTarget targetType);

        Task<(List<Review> Items, int TotalCount)> GetForStorePagedAsync(int storeId, int page, int pageSize);
        Task<(List<Review> Items, int TotalCount)> GetForDriverPagedAsync(int driverProfileId, int page, int pageSize);
        Task<List<Review>> GetByCustomerAsync(string customerId);

        // Average and count per store, for the stores on one page (one query, not one per store).
        Task<Dictionary<int, (double Average, int Count)>> GetStoreRatingsAsync(IEnumerable<int> storeIds);
        Task<(double? Average, int Count)> GetDriverRatingAsync(int driverProfileId);
    }
}
