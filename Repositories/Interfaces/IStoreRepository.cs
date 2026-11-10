using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IStoreRepository : IGenericRepository<Store>
    {
        // Approved stores only, filtered and paged. Returns the page and the total count.
        Task<(List<Store> Items, int TotalCount)> GetApprovedPagedAsync(StoreCategory? category, string? search, int page, int pageSize);
        Task<List<Store>> GetByOwnerAsync(string ownerId);
        Task<List<Store>> GetPendingAsync();
        // Store with its menu categories and their products.
        Task<Store?> GetWithMenuAsync(int id);
    }
}
