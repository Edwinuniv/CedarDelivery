using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CedarDelivery.Constants;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services;

namespace CedarDelivery.Controllers
{
    // Every MVC controller inherits from this.
    // - Before each action it loads the signed-in user's name and picture into ViewBag
    //   so _Layout.cshtml can show them in the sidebar and top bar.
    // - AppExceptionPageFilter turns service errors into friendly pages and messages.
    [TypeFilter(typeof(AppExceptionPageFilter))]
    public class BaseController : Controller
    {
        protected readonly UserManager<User> _userManager;
        protected readonly IUserRepository _userRepository;

        public BaseController(UserManager<User> userManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }

        // The signed-in user's Id, or null for visitors.
        protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Use only inside [Authorize] actions, where a user is always signed in.
        protected string RequiredUserId => CurrentUserId
            ?? throw new InvalidOperationException("RequiredUserId used outside an [Authorize] action.");

        protected bool IsAdmin => User.IsInRole(Roles.Admin);

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

        // For form posts: runs a service call, shows a green message on success
        // or a red one when the service refuses (BadRequestException).
        // Returns true on success so the action can choose where to redirect.
        protected async Task<bool> TryAsync(Func<Task> action, string? successMessage = null)
        {
            try
            {
                await action();
                if (successMessage != null)
                {
                    TempData["Success"] = successMessage;
                }
                return true;
            }
            catch (BadRequestException ex)
            {
                TempData["Error"] = ex.Message;
                return false;
            }
        }

        // Redirects to a local URL from a form (e.g. "come back to this store"), or to a fallback.
        protected IActionResult RedirectToLocal(string? returnUrl, string fallbackAction, string? fallbackController = null, object? routeValues = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(fallbackAction, fallbackController, routeValues);
        }

        private static string FirstLetter(string? value)
        {
            return string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);
        }
    }
}
