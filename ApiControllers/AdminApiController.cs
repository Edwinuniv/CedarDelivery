using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // /api/admin: users, dashboard numbers and the activity log. Admins only.
    // (Store approval, applications and all orders live in their own controllers.)
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminApiController : BaseApiController
    {
        private readonly IAdminService _adminService;
        private readonly IActivityLogService _activityLogService;

        public AdminApiController(IAdminService adminService, IActivityLogService activityLogService)
        {
            _adminService = adminService;
            _activityLogService = activityLogService;
        }

        // GET /api/admin/users?search=alice&role=Driver&page=1
        [HttpGet("users")]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] UserQuery query)
        {
            return Ok(await _adminService.GetUsersAsync(query));
        }

        // GET /api/admin/users/{id}
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            return Ok(await _adminService.GetUserAsync(id));
        }

        // PATCH /api/admin/users/{id}/active   { "isActive": false }
        [HttpPatch("users/{id}/active")]
        public async Task<ActionResult<UserDto>> SetActive(string id, SetUserActiveRequest request)
        {
            return Ok(await _adminService.SetUserActiveAsync(id, RequiredUserId, request.IsActive!.Value));
        }

        // GET /api/admin/analytics?days=30
        [HttpGet("analytics")]
        public async Task<ActionResult<AnalyticsDto>> GetAnalytics([FromQuery] AnalyticsQuery query)
        {
            return Ok(await _adminService.GetAnalyticsAsync(query));
        }

        // GET /api/admin/activity?userId=...&action=Login&page=1
        [HttpGet("activity")]
        public async Task<ActionResult<PagedResult<ActivityLogDto>>> GetActivity([FromQuery] ActivityQuery query)
        {
            return Ok(await _activityLogService.GetAllAsync(query));
        }
    }
}
