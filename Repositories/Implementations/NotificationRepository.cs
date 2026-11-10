using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<(List<Notification> Items, int TotalCount)> GetByUserPagedAsync(string userId, int page, int pageSize)
        {
            var query = _context.Notifications.Where(n => n.UserId == userId);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> CountUnreadAsync(string userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAllReadAsync(string userId)
        {
            // ExecuteUpdateAsync runs one UPDATE statement without loading the rows.
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }
    }
}
