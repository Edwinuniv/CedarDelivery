using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Constants;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Controllers
{
    // Driver pages. The rules (approved + online, one job at a time, pickup before delivery)
    // live in DeliveryService.
    [Authorize(Roles = Roles.Driver)]
    public class DeliveriesController : BaseController
    {
        private readonly IDeliveryService _deliveryService;
        private readonly IReviewService _reviewService;

        public DeliveriesController(UserManager<User> userManager, IUserRepository userRepository,
            IDeliveryService deliveryService, IReviewService reviewService)
            : base(userManager, userRepository)
        {
            _deliveryService = deliveryService;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index()
        {
            var profile = await _deliveryService.GetProfileAsync(RequiredUserId);

            var model = new DriverDashboardViewModel
            {
                Profile = profile,
                Current = await _deliveryService.GetCurrentAsync(RequiredUserId),
                Earnings = await _deliveryService.GetEarningsAsync(RequiredUserId),
                Reviews = await _reviewService.GetDriverReviewsAsync(RequiredUserId, new PageQuery { PageSize = 3 })
            };

            // The job list only makes sense for an approved driver who is online and free.
            if (profile.IsApproved && profile.IsOnline && model.Current == null)
            {
                model.Available = (await _deliveryService.GetAvailableAsync(RequiredUserId, new PageQuery { PageSize = 20 })).Items;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetOnline(bool isOnline)
        {
            await TryAsync(() => _deliveryService.SetOnlineAsync(RequiredUserId, isOnline),
                isOnline ? "You're online. New jobs will appear here." : "You're offline.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int orderId)
        {
            await TryAsync(() => _deliveryService.AcceptAsync(orderId, RequiredUserId), "Job accepted. Head to the store!");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Release(int id)
        {
            await TryAsync(() => _deliveryService.ReleaseAsync(id, RequiredUserId), "Job given back.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> PickUp(int id)
        {
            await TryAsync(() => _deliveryService.PickUpAsync(id, RequiredUserId), "Picked up. The customer was notified.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deliver(int id)
        {
            await TryAsync(() => _deliveryService.DeliverAsync(id, RequiredUserId), "Delivered! Earnings updated.");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> History([FromQuery] DeliveryQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new DeliveryQuery();
            }
            query.PageSize = 20;
            ViewBag.Earnings = await _deliveryService.GetEarningsAsync(RequiredUserId);
            return View(await _deliveryService.GetMyDeliveriesAsync(RequiredUserId, query));
        }
    }
}
