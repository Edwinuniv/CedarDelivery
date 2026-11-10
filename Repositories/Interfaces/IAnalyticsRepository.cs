using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Repositories.Interfaces
{
    // Read-only queries for the admin dashboard. Kept apart from the entity repositories
    // because they read across many tables at once.
    public interface IAnalyticsRepository
    {
        Task<AnalyticsDto> GetDashboardAsync(DateTime since, int days);
    }
}
