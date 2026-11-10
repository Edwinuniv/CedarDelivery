using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // Menu management for merchants. Every method checks that userId owns the store
    // (or isAdmin). Customers read products through IStoreService.GetMenuAsync.
    public interface IProductService
    {
        // Categories
        Task<List<CategoryDto>> GetCategoriesAsync(int storeId, string userId, bool isAdmin);
        Task<CategoryDto> CreateCategoryAsync(int storeId, string userId, bool isAdmin, CategoryRequest request);
        Task<CategoryDto> UpdateCategoryAsync(int categoryId, string userId, bool isAdmin, CategoryRequest request);
        Task DeleteCategoryAsync(int categoryId, string userId, bool isAdmin);

        // Products
        Task<List<ProductDto>> GetStoreProductsAsync(int storeId, string userId, bool isAdmin);
        Task<ProductDto> CreateProductAsync(int storeId, string userId, bool isAdmin, ProductRequest request);
        Task<ProductDto> UpdateProductAsync(int productId, string userId, bool isAdmin, ProductRequest request);
        Task<ProductDto> SetAvailabilityAsync(int productId, string userId, bool isAdmin, bool isAvailable);
        Task DeleteProductAsync(int productId, string userId, bool isAdmin);
    }
}
