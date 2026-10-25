using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    // One line of an order. Name and price are copied from the product at order time,
    // so old orders stay correct when a product is edited or deleted.
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        // Null if the product was deleted later.
        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        [Required, MaxLength(100)]
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}
