using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // /api/notifications: the signed-in user's notifications (the bell icon).
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsApiController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationsApiController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET /api/notifications?page=1
        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationDto>>> GetMine([FromQuery] PageQuery query)
        {
            return Ok(await _notificationService.GetMineAsync(RequiredUserId, query));
        }

        // GET /api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(RequiredUserId);
            return Ok(new { unreadCount = count });
        }

        // PATCH /api/notifications/7/read
        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _notificationService.MarkReadAsync(id, RequiredUserId);
            return NoContent();
        }

        // PATCH /api/notifications/read-all
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(RequiredUserId);
            return NoContent();
        }
    }
}
