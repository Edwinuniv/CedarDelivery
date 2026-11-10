using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class DriverProfileRepository : GenericRepository<DriverProfile>, IDriverProfileRepository
    {
        public DriverProfileRepository(ApplicationDbContext context) : base(context) { }

        public async Task<DriverProfile?> GetByUserIdAsync(string userId)
        {
            return await _context.DriverProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<List<DriverProfile>> GetOnlineApprovedAsync()
        {
            return await _context.DriverProfiles
                .Where(d => d.IsOnline && d.IsApproved && d.User.IsActive)
                .ToListAsync();
        }
    }
}
