using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ViewModels
{
    // Menu management page for one store: categories, products and the two "add" forms.
    public class MenuManageViewModel
    {
        public StoreDto Store { get; set; } = null!;
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
        public CategoryRequest NewCategory { get; set; } = new CategoryRequest();
        public ProductRequest NewProduct { get; set; } = new ProductRequest();
    }

    // Create/edit store form. StoreId is null when creating.
    public class StoreFormViewModel
    {
        public int? StoreId { get; set; }
        public StoreRequest Store { get; set; } = new StoreRequest();
    }

    // Edit one product.
    public class ProductFormViewModel
    {
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public ProductRequest Product { get; set; } = new ProductRequest();
    }

    // A store's orders with a status filter.
    public class StoreOrdersViewModel
    {
        public StoreDto Store { get; set; } = null!;
        public OrderQuery Query { get; set; } = new OrderQuery();
        public PagedResult<OrderSummaryDto> Orders { get; set; } = new PagedResult<OrderSummaryDto>();
    }
}
