using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    public class Product
    {
        public int Id { get; set; }

        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        public int ProductCategoryId { get; set; }
        public ProductCategory ProductCategory { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        [MaxLength(1000)]
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        // The merchant can hide a product (e.g. out of stock) without deleting it.
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
