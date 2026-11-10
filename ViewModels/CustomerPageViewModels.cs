using CedarDelivery.Models;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ViewModels
{
    // Cart page: the cart, the user's addresses and the checkout form.
    public class CartPageViewModel
    {
        public CartDto Cart { get; set; } = new CartDto();
        public List<AddressDto> Addresses { get; set; } = new List<AddressDto>();
        public PlaceOrderRequest Checkout { get; set; } = new PlaceOrderRequest();
    }

    // Order page, shared by everyone involved. The flags decide which buttons appear.
    public class OrderDetailsViewModel
    {
        public OrderDetailsDto Order { get; set; } = null!;
        public bool IsCustomer { get; set; }
        public bool IsStoreOwner { get; set; }
        public bool IsAdmin { get; set; }

        // Merchant buttons, e.g. Pending -> Accepted / Rejected.
        public OrderStatus[] NextStoreStatuses { get; set; } = Array.Empty<OrderStatus>();

        public bool CanCancel { get; set; }
        public bool CanReviewStore { get; set; }
        public bool CanReviewDriver { get; set; }
    }

    // Addresses page: saved addresses and the "add address" form.
    public class AddressesPageViewModel
    {
        public List<AddressDto> Addresses { get; set; } = new List<AddressDto>();
        public AddressRequest NewAddress { get; set; } = new AddressRequest();
    }

    // "Sell or Deliver" page: past applications and the application form.
    public class ApplyPageViewModel
    {
        public List<RoleApplicationDto> Applications { get; set; } = new List<RoleApplicationDto>();
        public ApplyForRoleRequest Request { get; set; } = new ApplyForRoleRequest();
    }
}
