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
    }
}
