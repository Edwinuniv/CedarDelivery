using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IDriverProfileRepository : IGenericRepository<DriverProfile>
    {
        Task<DriverProfile?> GetByUserIdAsync(string userId);
        // Drivers who should hear about a new delivery.
        Task<List<DriverProfile>> GetOnlineApprovedAsync();
    }
}
