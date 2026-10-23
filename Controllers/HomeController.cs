using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(UserManager<User> userManager, IUserRepository userRepository)
            : base(userManager, userRepository)
        {
        }

        public IActionResult Index()
        {
            return View();
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
