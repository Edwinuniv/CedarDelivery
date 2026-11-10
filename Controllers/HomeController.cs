using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Constants;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IStoreService _storeService;

        public HomeController(UserManager<User> userManager, IUserRepository userRepository, IStoreService storeService)
            : base(userManager, userRepository)
        {
            _storeService = storeService;
        }

        // Visitors see the landing page. Signed-in users go straight to their main page.
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(Roles.Admin)) return RedirectToAction("Index", "Admin");
                if (User.IsInRole(Roles.Merchant)) return RedirectToAction("Index", "Merchant");
                if (User.IsInRole(Roles.Driver)) return RedirectToAction("Index", "Deliveries");
                return RedirectToAction("Index", "Stores");
            }

            var featured = await _storeService.GetStoresAsync(new StoreQuery { PageSize = 6 });
            return View(featured.Items);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
