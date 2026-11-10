using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task UpdateAsync(User user)
        {
            var existing = await _context.Users.FindAsync(user.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found.");
            }

            _context.Entry(existing).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<(List<User> Items, int TotalCount)> GetPagedAsync(string? search, string? role, int page, int pageSize)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u => u.FirstName.Contains(term)
                                      || u.LastName.Contains(term)
                                      || (u.Email != null && u.Email.Contains(term)));
            }

            // Identity keeps roles in two tables: AspNetUserRoles (links) and AspNetRoles (names).
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id
                    && _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Dictionary<string, List<string>>> GetRolesForUsersAsync(IEnumerable<string> userIds)
        {
            var ids = userIds.ToList();

            var rows = await (from ur in _context.UserRoles
                              join r in _context.Roles on ur.RoleId equals r.Id
                              where ids.Contains(ur.UserId)
                              select new { ur.UserId, r.Name })
                             .ToListAsync();

            return rows
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Name!).OrderBy(n => n).ToList());
        }
    }
}
