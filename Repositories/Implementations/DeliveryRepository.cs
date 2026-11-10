using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
    {
        public DeliveryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Delivery?> GetWithOrderAsync(int id)
        {
            return await _context.Deliveries
                .Include(d => d.Order).ThenInclude(o => o.Store)
                .Include(d => d.Order).ThenInclude(o => o.Customer)
                .Include(d => d.Order).ThenInclude(o => o.StatusHistory)
                .Include(d => d.DriverProfile).ThenInclude(p => p.User)
                .AsSplitQuery()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Delivery?> GetActiveByDriverAsync(int driverProfileId)
        {
            return await _context.Deliveries
                .Include(d => d.Order).ThenInclude(o => o.Store)
                .Include(d => d.Order).ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(d => d.DriverProfileId == driverProfileId
                    && (d.Status == DeliveryStatus.Assigned || d.Status == DeliveryStatus.PickedUp));
        }

        public async Task<(List<Delivery> Items, int TotalCount)> GetByDriverPagedAsync(int driverProfileId, DeliveryStatus? status, int page, int pageSize)
        {
            var query = _context.Deliveries.Where(d => d.DriverProfileId == driverProfileId);
            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(d => d.Order).ThenInclude(o => o.Store)
                .Include(d => d.Order).ThenInclude(o => o.Customer)
                .OrderByDescending(d => d.AssignedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(decimal Total, int Count)> GetEarningsAsync(int driverProfileId, DateTime? since)
        {
            var query = _context.Deliveries.Where(d =>
                d.DriverProfileId == driverProfileId && d.Status == DeliveryStatus.Delivered);

            if (since.HasValue)
            {
                query = query.Where(d => d.DeliveredAt >= since.Value);
            }

            // (decimal?) so SUM over zero rows gives null -> 0 instead of an error.
            var total = await query.SumAsync(d => (decimal?)d.DriverEarning) ?? 0;
            var count = await query.CountAsync();
            return (total, count);
        }

        public async Task<bool> TryAssignAsync(Delivery delivery)
        {
            _context.Deliveries.Add(delivery);
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                // Lost the race: forget the failed insert so the context stays usable.
                _context.Entry(delivery).State = EntityState.Detached;
                return false;
            }
        }
    }
}
