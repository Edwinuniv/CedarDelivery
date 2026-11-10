using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    // Users have string Ids (from Identity), so they get their own repository
    // instead of IGenericRepository<T>, which uses int Ids.
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task UpdateAsync(User user);
        Task<bool> ExistsAsync(string id);

        // Admin user list: search by name or email, optional role filter, newest first.
        Task<(List<User> Items, int TotalCount)> GetPagedAsync(string? search, string? role, int page, int pageSize);

        // Role names for many users in one query (userId -> roles).
        Task<Dictionary<string, List<string>>> GetRolesForUsersAsync(IEnumerable<string> userIds);
    }
}
