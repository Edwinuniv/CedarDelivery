using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IStoreRepository _storeRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _categoryRepository;
        private readonly IActivityLogService _activityLog;

        public ProductService(
            IStoreRepository storeRepository,
            IProductRepository productRepository,
            IProductCategoryRepository categoryRepository,
            IActivityLogService activityLog)
        {
            _storeRepository = storeRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _activityLog = activityLog;
        }

        // ---------- Categories ----------

        public async Task<List<CategoryDto>> GetCategoriesAsync(int storeId, string userId, bool isAdmin)
        {
            await EnsureCanManageStoreAsync(storeId, userId, isAdmin);

            var categories = await _categoryRepository.GetByStoreAsync(storeId);
            return categories.Select(CategoryDto.FromEntity).ToList();
        }

        public async Task<CategoryDto> CreateCategoryAsync(int storeId, string userId, bool isAdmin, CategoryRequest request)
        {
            await EnsureCanManageStoreAsync(storeId, userId, isAdmin);

            var category = new ProductCategory
            {
                StoreId = storeId,
                Name = request.Name.Trim(),
                DisplayOrder = request.DisplayOrder
            };

            await _categoryRepository.AddAsync(category);
            await _activityLog.LogAsync(userId, ActivityType.ProductChanged, $"Added category \"{category.Name}\" to store #{storeId}.");

            return CategoryDto.FromEntity(category);
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int categoryId, string userId, bool isAdmin, CategoryRequest request)
        {
            var category = await GetCategoryToManageAsync(categoryId, userId, isAdmin);

            category.Name = request.Name.Trim();
            category.DisplayOrder = request.DisplayOrder;

            await _categoryRepository.UpdateAsync(category);
            await _activityLog.LogAsync(userId, ActivityType.ProductChanged, $"Updated category \"{category.Name}\" (#{category.Id}).");

            return CategoryDto.FromEntity(category);
        }

        public async Task DeleteCategoryAsync(int categoryId, string userId, bool isAdmin)
        {
            var category = await GetCategoryToManageAsync(categoryId, userId, isAdmin);

            // The database also blocks this (DeleteBehavior.Restrict); checking first
            // gives the merchant a clear message instead of a server error.
            if (await _categoryRepository.HasProductsAsync(categoryId))
            {
                throw new BadRequestException("Move or delete the products in this category first.");
            }

            await _categoryRepository.DeleteAsync(categoryId);
            await _activityLog.LogAsync(userId, ActivityType.ProductChanged, $"Deleted category \"{category.Name}\" (#{category.Id}).");
        }

        // ---------- Products ----------

        public async Task<List<ProductDto>> GetStoreProductsAsync(int storeId, string userId, bool isAdmin)
        {
            await EnsureCanManageStoreAsync(storeId, userId, isAdmin);

            var products = await _productRepository.GetByStoreAsync(storeId);
            return products.Select(ProductDto.FromEntity).ToList();
        }

        public async Task<ProductDto> CreateProductAsync(int storeId, string userId, bool isAdmin, ProductRequest request)
        {
            await EnsureCanManageStoreAsync(storeId, userId, isAdmin);
            var category = await GetCategoryOfStoreAsync(request.ProductCategoryId!.Value, storeId);

            var product = new Product
            {
                StoreId = storeId,
                CreatedAt = DateTime.Now
            };
            ApplyRequest(product, request, category);

            await _productRepository.AddAsync(product);
            await _activityLog.LogAsync(userId, ActivityType.ProductChanged, $"Added product \"{product.Name}\" to store #{storeId}.");

            return ProductDto.FromEntity(product);
        }

        public async Task<ProductDto> UpdateProductAsync(int productId, string userId, bool isAdmin, ProductRequest request)
        {
            var product = await GetProductToManageAsync(productId, userId, isAdmin);
            var category = await GetCategoryOfStoreAsync(request.ProductCategoryId!.Value, product.StoreId);

            ApplyRequest(product, request, category);

            await _productRepository.UpdateAsync(product);
            await _activityLog.LogAsync(userId, ActivityType.ProductChanged, $"Updated product \"{product.Name}\" (#{product.Id}).");

            return ProductDto.FromEntity(product);
        }

        public async Task<ProductDto> SetAvailabilityAsync(int productId, string userId, bool isAdmin, bool isAvailable)
        {
            var product = await GetProductToManageAsync(productId, userId, isAdmin);

            product.IsAvailable = isAvailable;

            await _productRepository.UpdateAsync(product);
            await _activityLog.LogAsync(userId, ActivityType.ProductChanged,
                $"Marked product \"{product.Name}\" (#{product.Id}) as {(isAvailable ? "available" : "unavailable")}.");

            return ProductDto.FromEntity(product);
        }

        public async Task DeleteProductAsync(int productId, string userId, bool isAdmin)
        {
            var product = await GetProductToManageAsync(productId, userId, isAdmin);

            // Cart items with this product are removed (Cascade); past order items
            // keep their copied name and price (SetNull), so order history is safe.
            await _productRepository.DeleteAsync(productId);
            await _activityLog.LogAsync(userId, ActivityType.ProductChanged, $"Deleted product \"{product.Name}\" (#{product.Id}).");
        }

        // ---------- Helpers ----------

        // Throws unless the store exists and belongs to the user (admins can manage any store).
        private async Task EnsureCanManageStoreAsync(int storeId, string userId, bool isAdmin)
        {
            var store = await _storeRepository.GetByIdAsync(storeId)
                ?? throw new NotFoundException("Store not found.");

            if (store.OwnerId != userId && !isAdmin)
            {
                throw new ForbiddenException("You can only manage your own stores.");
            }
        }

        private async Task<ProductCategory> GetCategoryToManageAsync(int categoryId, string userId, bool isAdmin)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId)
                ?? throw new NotFoundException("Category not found.");

            await EnsureCanManageStoreAsync(category.StoreId, userId, isAdmin);
            return category;
        }

        private async Task<Product> GetProductToManageAsync(int productId, string userId, bool isAdmin)
        {
            var product = await _productRepository.GetWithCategoryAsync(productId)
                ?? throw new NotFoundException("Product not found.");

            await EnsureCanManageStoreAsync(product.StoreId, userId, isAdmin);
            return product;
        }

        // A product can only go in a category of its own store.
        private async Task<ProductCategory> GetCategoryOfStoreAsync(int categoryId, int storeId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);

            if (category == null || category.StoreId != storeId)
            {
                throw new BadRequestException("The category does not belong to this store.");
            }

            return category;
        }

        private static void ApplyRequest(Product product, ProductRequest request, ProductCategory category)
        {
            product.ProductCategoryId = category.Id;
            product.ProductCategory = category;
            product.Name = request.Name.Trim();
            product.Description = request.Description?.Trim();
            product.Price = request.Price!.Value;
            product.ImageUrl = request.ImageUrl;
            product.IsAvailable = request.IsAvailable;
        }
    }
}
