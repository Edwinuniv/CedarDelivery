using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<(List<Notification> Items, int TotalCount)> GetByUserPagedAsync(string userId, int page, int pageSize);
        Task<int> CountUnreadAsync(string userId);
        Task MarkAllReadAsync(string userId);
    }
}
