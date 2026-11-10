using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        // All products of a store (available or not), with their category.
        Task<List<Product>> GetByStoreAsync(int storeId);
        Task<Product?> GetWithCategoryAsync(int id);
    }
}
