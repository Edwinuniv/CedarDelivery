using CedarDelivery.Models;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    public interface IActivityLogService
    {
        // Records what a user did, with their IP address and browser.
        Task LogAsync(string userId, ActivityType action, string? details = null);

        // A user's own history, and the admin view of everyone's.
        Task<PagedResult<ActivityLogDto>> GetMineAsync(string userId, PageQuery query);
        Task<PagedResult<ActivityLogDto>> GetAllAsync(ActivityQuery query);
    }
}
