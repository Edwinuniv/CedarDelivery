using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Controllers
{
    // "My orders" and the order page. The order page is shared by the customer,
    // the store owner, the driver and admins; each sees only their own buttons.
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IReviewService _reviewService;

        public OrdersController(UserManager<User> userManager, IUserRepository userRepository,
            IOrderService orderService, IReviewService reviewService)
            : base(userManager, userRepository)
        {
            _orderService = orderService;
            _reviewService = reviewService;
        }

        // GET /Orders?page=2
        public async Task<IActionResult> Index([FromQuery] PageQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new PageQuery();
            }
            return View(await _orderService.GetMyOrdersAsync(RequiredUserId, query));
        }

        // GET /Orders/Details/15
        public async Task<IActionResult> Details(int id)
        {
            // GetOrderAsync throws NotFound if this user isn't involved in the order.
            var order = await _orderService.GetOrderAsync(id, RequiredUserId, IsAdmin);

            var isCustomer = order.CustomerId == RequiredUserId;
            var isStoreOwner = order.StoreOwnerId == RequiredUserId;
            var delivered = order.Status == OrderStatus.Delivered;

            return View(new OrderDetailsViewModel
            {
                Order = order,
                IsCustomer = isCustomer,
                IsStoreOwner = isStoreOwner,
                IsAdmin = IsAdmin,
                NextStoreStatuses = isStoreOwner || IsAdmin
                    ? OrderStatusRules.NextMerchantStatuses(order.Status)
                    : Array.Empty<OrderStatus>(),
                // Customers cancel while Pending; admins until the driver picks it up.
                CanCancel = (isCustomer && order.Status == OrderStatus.Pending)
                            || (IsAdmin && !OrderStatusRules.IsFinal(order.Status) && order.Status != OrderStatus.PickedUp),
                CanReviewStore = isCustomer && delivered && !order.ReviewedTargets.Contains(ReviewTarget.Store),
                CanReviewDriver = isCustomer && delivered && order.Driver != null && !order.ReviewedTargets.Contains(ReviewTarget.Driver)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            await TryAsync(() => _orderService.CancelAsync(id, RequiredUserId, IsAdmin, reason), "The order was cancelled.");
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Policy = "MerchantOrAdmin")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string? note)
        {
            await TryAsync(() => _orderService.UpdateStatusByStoreAsync(id, RequiredUserId, IsAdmin, status, note),
                $"Order marked as {Ui.Words(status).ToLower()}.");
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Review(int id, ReviewTarget targetType, int rating, string? comment)
        {
            await TryAsync(() => _reviewService.CreateAsync(RequiredUserId, new CreateReviewRequest
            {
                OrderId = id,
                TargetType = targetType,
                Rating = Math.Clamp(rating, 1, 5),
                Comment = comment
            }), "Thanks for your review!");
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
