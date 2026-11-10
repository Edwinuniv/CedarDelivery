using CedarDelivery.Models;

namespace CedarDelivery.Repositories.Interfaces
{
    public interface IProductCategoryRepository : IGenericRepository<ProductCategory>
    {
        Task<List<ProductCategory>> GetByStoreAsync(int storeId);
        Task<bool> HasProductsAsync(int categoryId);
    }
}
