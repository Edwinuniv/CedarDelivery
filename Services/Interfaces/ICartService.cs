using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // The signed-in user's cart. Every method returns the updated cart.
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(string userId);
        Task<CartDto> AddItemAsync(string userId, AddCartItemRequest request);
        Task<CartDto> UpdateItemAsync(int itemId, string userId, int quantity);
        Task<CartDto> RemoveItemAsync(int itemId, string userId);
        Task<CartDto> ClearAsync(string userId);
    }
}
