using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Controllers
{
    // The cart page and checkout. All rules (one store per cart, minimum order,
    // store open, cash only) are in CartService and OrderService.
    [Authorize]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        private readonly IAddressService _addressService;
        private readonly IOrderService _orderService;

        public CartController(UserManager<User> userManager, IUserRepository userRepository,
            ICartService cartService, IAddressService addressService, IOrderService orderService)
            : base(userManager, userRepository)
        {
            _cartService = cartService;
            _addressService = addressService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var addresses = await _addressService.GetMineAsync(RequiredUserId);

            return View(new CartPageViewModel
            {
                Cart = await _cartService.GetCartAsync(RequiredUserId),
                Addresses = addresses,
                Checkout = new PlaceOrderRequest
                {
                    AddressId = addresses.FirstOrDefault(a => a.IsDefault)?.Id,
                    PaymentMethod = PaymentMethod.CashOnDelivery
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
        {
            await TryAsync(() => _cartService.AddItemAsync(RequiredUserId,
                new AddCartItemRequest { ProductId = productId, Quantity = Math.Clamp(quantity, 1, 50) }),
                "Added to your cart.");

            // Stay on the store page so the customer can keep adding.
            return RedirectToLocal(returnUrl, nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, int quantity)
        {
            await TryAsync(() => _cartService.UpdateItemAsync(id, RequiredUserId, Math.Clamp(quantity, 0, 50)));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            await TryAsync(() => _cartService.RemoveItemAsync(id, RequiredUserId), "Item removed.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await TryAsync(() => _cartService.ClearAsync(RequiredUserId), "Your cart is empty.");
            return RedirectToAction(nameof(Index));
        }

        // The form fields are named "Checkout.AddressId", etc., hence the prefix.
        [HttpPost]
        public async Task<IActionResult> Checkout([Bind(Prefix = "Checkout")] PlaceOrderRequest checkout)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Choose a delivery address and a payment method.";
                return RedirectToAction(nameof(Index));
            }

            OrderDetailsDto? order = null;
            var placed = await TryAsync(async () => order = await _orderService.PlaceOrderAsync(RequiredUserId, checkout),
                "Order placed! The store will confirm it soon.");

            return placed
                ? RedirectToAction("Details", "Orders", new { id = order!.Id })
                : RedirectToAction(nameof(Index));
        }
    }
}
