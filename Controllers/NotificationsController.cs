using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Controllers
{
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(UserManager<User> userManager, IUserRepository userRepository, INotificationService notificationService)
            : base(userManager, userRepository)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index([FromQuery] PageQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new PageQuery();
            }
            query.PageSize = 20;
            return View(await _notificationService.GetMineAsync(RequiredUserId, query));
        }

        // Clicking a notification marks it read, then opens the page it points to.
        public async Task<IActionResult> Open(int id)
        {
            var notification = await _notificationService.MarkReadAsync(id, RequiredUserId);
            return RedirectToLocal(notification.RedirectUrl, nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ReadAll()
        {
            await _notificationService.MarkAllReadAsync(RequiredUserId);
            return RedirectToAction(nameof(Index));
        }
    }
}
