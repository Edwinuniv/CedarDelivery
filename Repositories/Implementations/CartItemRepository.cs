using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<CartItem>> GetByUserAsync(string userId)
        {
            return await _context.CartItems
                .Include(c => c.Product)
                    .ThenInclude(p => p.Store)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.AddedAt)
                .ToListAsync();
        }

        public async Task<CartItem?> GetByUserAndProductAsync(string userId, int productId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
        }

        public async Task ClearAsync(string userId)
        {
            var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
