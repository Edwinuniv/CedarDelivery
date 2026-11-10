using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.ViewModels.Api
{
    // The customer's cart with prices worked out. Warnings explain why checkout
    // would fail right now (store closed, item unavailable, below minimum order).
    public class CartDto
    {
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
        public decimal MinimumOrder { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public bool CanCheckout => Items.Count > 0 && Warnings.Count == 0;
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class AddCartItemRequest
    {
        [Required]
        public int? ProductId { get; set; }

        [Range(1, 50)]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        // 0 removes the item.
        [Range(0, 50)]
        public int Quantity { get; set; }
    }
}
