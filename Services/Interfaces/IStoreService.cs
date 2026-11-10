using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // userId is the signed-in user (null for visitors); isAdmin lets admins manage any store.
    public interface IStoreService
    {
        // Public browsing (approved stores only).
        Task<PagedResult<StoreDto>> GetStoresAsync(StoreQuery query);
        Task<StoreDto> GetStoreAsync(int id, string? userId, bool isAdmin);
        Task<MenuDto> GetMenuAsync(int id, string? userId, bool isAdmin);

        // Merchant.
        Task<List<StoreDto>> GetMyStoresAsync(string userId);
        Task<StoreDto> CreateStoreAsync(string userId, StoreRequest request);
        Task<StoreDto> UpdateStoreAsync(int id, string userId, bool isAdmin, StoreRequest request);
        Task<StoreDto> SetOpenAsync(int id, string userId, bool isAdmin, bool isOpen);

        // Admin.
        Task<List<StoreDto>> GetPendingStoresAsync();
        Task<StoreDto> SetApprovalAsync(int id, string adminId, bool isApproved);
    }
}
