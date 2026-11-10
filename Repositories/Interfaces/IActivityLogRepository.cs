using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IActivityLogRepository : IGenericRepository<ActivityLog>
    {
        // Newest first. Both filters optional.
        Task<(List<ActivityLog> Items, int TotalCount)> GetPagedAsync(string? userId, ActivityType? action, int page, int pageSize);
    }
}
