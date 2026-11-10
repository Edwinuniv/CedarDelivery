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
    // Browsing stores and menus. Open to visitors; adding to the cart needs a login.
    [AllowAnonymous]
    public class StoresController : BaseController
    {
        private readonly IStoreService _storeService;
        private readonly IReviewService _reviewService;

        public StoresController(UserManager<User> userManager, IUserRepository userRepository,
            IStoreService storeService, IReviewService reviewService)
            : base(userManager, userRepository)
        {
            _storeService = storeService;
            _reviewService = reviewService;
        }

        // GET /Stores?search=pizza&category=Restaurant&page=2
        public async Task<IActionResult> Index([FromQuery] StoreQuery query)
        {
            // A hand-edited URL (page=0, pageSize=500) falls back to the defaults.
            if (!ModelState.IsValid)
            {
                query = new StoreQuery { Search = query.Search, Category = query.Category };
            }
            query.PageSize = 12;

            ViewBag.Query = query;
            return View(await _storeService.GetStoresAsync(query));
        }

        // GET /Stores/Details/5   (the menu)
        public async Task<IActionResult> Details(int id)
        {
            var menu = await _storeService.GetMenuAsync(id, CurrentUserId, IsAdmin);

            var model = new StorePageViewModel { Menu = menu };

            // Reviews are public only once the store is approved.
            if (menu.Store.IsApproved)
            {
                model.Reviews = await _reviewService.GetStoreReviewsAsync(id, new PageQuery { PageSize = 5 });
            }

            return View(model);
        }
    }
}
