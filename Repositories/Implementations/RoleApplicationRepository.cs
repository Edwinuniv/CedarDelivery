using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class RoleApplicationRepository : GenericRepository<RoleApplication>, IRoleApplicationRepository
    {
        public RoleApplicationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<RoleApplication>> GetByUserAsync(string userId)
        {
            return await _context.RoleApplications
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<(List<RoleApplication> Items, int TotalCount)> GetPagedAsync(ApplicationStatus? status, int page, int pageSize)
        {
            var query = _context.RoleApplications.AsQueryable();
            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            // Oldest first, so admins handle applications in the order they arrived.
            var items = await query
                .Include(a => a.User)
                .OrderBy(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<RoleApplication?> GetWithUserAsync(int id)
        {
            return await _context.RoleApplications
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> HasPendingAsync(string userId, RequestedRole role)
        {
            return await _context.RoleApplications.AnyAsync(a =>
                a.UserId == userId && a.RequestedRole == role && a.Status == ApplicationStatus.Pending);
        }
    }
}
