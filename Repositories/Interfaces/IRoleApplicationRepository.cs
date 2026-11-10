using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IRoleApplicationRepository : IGenericRepository<RoleApplication>
    {
        Task<List<RoleApplication>> GetByUserAsync(string userId);
        Task<(List<RoleApplication> Items, int TotalCount)> GetPagedAsync(ApplicationStatus? status, int page, int pageSize);
        Task<RoleApplication?> GetWithUserAsync(int id);
        Task<bool> HasPendingAsync(string userId, RequestedRole role);
    }
}
