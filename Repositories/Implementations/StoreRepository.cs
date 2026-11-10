using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class StoreRepository : GenericRepository<Store>, IStoreRepository
    {
        public StoreRepository(ApplicationDbContext context) : base(context) { }

        public async Task<(List<Store> Items, int TotalCount)> GetApprovedPagedAsync(StoreCategory? category, string? search, int page, int pageSize)
        {
            // Build the query step by step; nothing runs on the database until CountAsync/ToListAsync.
            var query = _context.Stores.Where(s => s.IsApproved);

            if (category.HasValue)
            {
                query = query.Where(s => s.Category == category.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s => s.Name.Contains(term));
            }

            var totalCount = await query.CountAsync();

            // Open stores first, then alphabetical.
            var items = await query
                .OrderByDescending(s => s.IsOpen)
                .ThenBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Store>> GetByOwnerAsync(string ownerId)
        {
            return await _context.Stores
                .Where(s => s.OwnerId == ownerId)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<List<Store>> GetPendingAsync()
        {
            return await _context.Stores
                .Where(s => !s.IsApproved)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Store?> GetWithMenuAsync(int id)
        {
            return await _context.Stores
                .Include(s => s.ProductCategories)
                    .ThenInclude(c => c.Products)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
