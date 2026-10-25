using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    // A section of a store's menu, e.g. "Burgers" or "Drinks".
    public class ProductCategory
    {
        public int Id { get; set; }

        public int StoreId { get; set; }
        public Store Store { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        // Lower numbers are shown first on the menu.
        public int DisplayOrder { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
