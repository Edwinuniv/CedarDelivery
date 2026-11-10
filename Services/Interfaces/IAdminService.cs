using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // Admin tools: users and the dashboard numbers.
    public interface IAdminService
    {
        Task<PagedResult<UserDto>> GetUsersAsync(UserQuery query);
        Task<UserDto> GetUserAsync(string userId);
        Task<UserDto> SetUserActiveAsync(string userId, string adminId, bool isActive);
        Task<AnalyticsDto> GetAnalyticsAsync(AnalyticsQuery query);
    }
}
