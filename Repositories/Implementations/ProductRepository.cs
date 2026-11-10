using Microsoft.EntityFrameworkCore;
using CedarDelivery.Data;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Repositories.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<Product>> GetByStoreAsync(int storeId)
        {
            return await _context.Products
                .Include(p => p.ProductCategory)
                .Where(p => p.StoreId == storeId)
                .OrderBy(p => p.ProductCategory.DisplayOrder)
                .ThenBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetWithCategoryAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
