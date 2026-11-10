using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface ICartItemRepository : IGenericRepository<CartItem>
    {
        // The user's cart items with each product and its store.
        Task<List<CartItem>> GetByUserAsync(string userId);
        Task<CartItem?> GetByUserAndProductAsync(string userId, int productId);
        Task ClearAsync(string userId);
    }
}
