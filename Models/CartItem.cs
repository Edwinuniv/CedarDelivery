using System.ComponentModel.DataAnnotations;

namespace CedarDelivery.Models
{
    // One product in a customer's cart. The cart is saved in the database so it
    // survives logout and works the same from the website and the API.
    // Rule (enforced in the service layer): all items must come from one store.
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public User User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
