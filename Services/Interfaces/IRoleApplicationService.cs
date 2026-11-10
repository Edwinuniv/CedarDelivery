using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // Customers apply to become Merchants or Drivers; Admins approve or reject.
    public interface IRoleApplicationService
    {
        Task<RoleApplicationDto> ApplyAsync(string userId, ApplyForRoleRequest request);
        Task<List<RoleApplicationDto>> GetMineAsync(string userId);

        // Admin
        Task<PagedResult<RoleApplicationDto>> GetApplicationsAsync(ApplicationQuery query);
        Task<RoleApplicationDto> ReviewAsync(int id, string adminId, bool approve, string? note);
    }
}
