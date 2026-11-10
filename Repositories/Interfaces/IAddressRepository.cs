using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IAddressRepository : IGenericRepository<Address>
    {
        // Default address first.
        Task<List<Address>> GetByUserAsync(string userId);
    }
}
