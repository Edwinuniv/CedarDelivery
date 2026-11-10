using System.ComponentModel.DataAnnotations;
using CedarDelivery.Models;

namespace CedarDelivery.ViewModels.Api
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int ProductCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }

        // ProductCategory must be loaded (Include) for CategoryName to be filled.
        public static ProductDto FromEntity(Product product) => new ProductDto
        {
            Id = product.Id,
            StoreId = product.StoreId,
            ProductCategoryId = product.ProductCategoryId,
            CategoryName = product.ProductCategory?.Name,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            IsAvailable = product.IsAvailable
        };
    }

    public class ProductRequest
    {
        [Required]
        public int? ProductCategoryId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required, Range(0.01, 10000)]
        public decimal? Price { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    public class SetProductAvailabilityRequest
    {
        [Required]
        public bool? IsAvailable { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public string Name { get; set; } = "";
        public int DisplayOrder { get; set; }

        public static CategoryDto FromEntity(ProductCategory category) => new CategoryDto
        {
            Id = category.Id,
            StoreId = category.StoreId,
            Name = category.Name,
            DisplayOrder = category.DisplayOrder
        };
    }

    public class CategoryRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [Range(0, 1000)]
        public int DisplayOrder { get; set; }
    }
}
