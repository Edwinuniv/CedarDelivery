using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class ActivityLogRepository : GenericRepository<ActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(ApplicationDbContext context) : base(context) { }

        public async Task<(List<ActivityLog> Items, int TotalCount)> GetPagedAsync(string? userId, ActivityType? action, int page, int pageSize)
        {
            var query = _context.ActivityLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId);
            }
            if (action.HasValue)
            {
                query = query.Where(a => a.Action == action.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
