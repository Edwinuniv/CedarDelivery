using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(int orderId, ReviewTarget targetType)
        {
            return await _context.Reviews.AnyAsync(r => r.OrderId == orderId && r.TargetType == targetType);
        }

        public async Task<(List<Review> Items, int TotalCount)> GetForStorePagedAsync(int storeId, int page, int pageSize)
        {
            return await PageAsync(_context.Reviews.Where(r => r.StoreId == storeId), page, pageSize);
        }

        public async Task<(List<Review> Items, int TotalCount)> GetForDriverPagedAsync(int driverProfileId, int page, int pageSize)
        {
            return await PageAsync(_context.Reviews.Where(r => r.DriverProfileId == driverProfileId), page, pageSize);
        }

        public async Task<List<Review>> GetByCustomerAsync(string customerId)
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<int, (double Average, int Count)>> GetStoreRatingsAsync(IEnumerable<int> storeIds)
        {
            var ids = storeIds.ToList();

            // GROUP BY StoreId: one row per store with its average and count.
            var rows = await _context.Reviews
                .Where(r => r.StoreId != null && ids.Contains(r.StoreId.Value))
                .GroupBy(r => r.StoreId!.Value)
                .Select(g => new { StoreId = g.Key, Average = g.Average(r => (double)r.Rating), Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(r => r.StoreId, r => (Math.Round(r.Average, 1), r.Count));
        }

        public async Task<(double? Average, int Count)> GetDriverRatingAsync(int driverProfileId)
        {
            var query = _context.Reviews.Where(r => r.DriverProfileId == driverProfileId);
            var count = await query.CountAsync();
            if (count == 0)
            {
                return (null, 0);
            }
            var average = await query.AverageAsync(r => (double)r.Rating);
            return (Math.Round(average, 1), count);
        }

        private static async Task<(List<Review> Items, int TotalCount)> PageAsync(IQueryable<Review> query, int page, int pageSize)
        {
            var totalCount = await query.CountAsync();
            var items = await query
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalCount);
        }
    }
}
