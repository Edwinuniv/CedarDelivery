using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;

namespace CedarDelivery.Controllers
{
    // Every MVC controller inherits from this.
    // Before each action it loads the signed-in user's name and picture into ViewBag
    // so _Layout.cshtml can show them in the sidebar and top bar.
    public class BaseController : Controller
    {
        protected readonly UserManager<User> _userManager;
        protected readonly IUserRepository _userRepository;

        public BaseController(UserManager<User> userManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        ViewBag.ProfilePictureUrl = user.ProfilePictureUrl;
                        ViewBag.FullName = $"{user.FirstName} {user.LastName}";
                        // Guard against empty names so Substring never throws.
                        ViewBag.UserInitials = $"{FirstLetter(user.FirstName)}{FirstLetter(user.LastName)}";
                    }
                }
            }

            await next();
        }

        private static string FirstLetter(string? value)
        {
            return string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);
        }
    }
}
