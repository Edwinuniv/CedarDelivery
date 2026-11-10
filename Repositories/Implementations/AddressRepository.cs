using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        public AddressRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<Address>> GetByUserAsync(string userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Label)
                .ToListAsync();
        }
    }
}
