using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // /api/account: the signed-in user's own account info and activity history.
    [Route("api/account")]
    [Authorize]
    public class AccountApiController : BaseApiController
    {
        private readonly IAdminService _adminService;
        private readonly IActivityLogService _activityLogService;

        public AccountApiController(IAdminService adminService, IActivityLogService activityLogService)
        {
            _adminService = adminService;
            _activityLogService = activityLogService;
        }

        // GET /api/account/me   (name, email, roles)
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            return Ok(await _adminService.GetUserAsync(RequiredUserId));
        }

        // GET /api/account/activity?page=1
        [HttpGet("activity")]
        public async Task<ActionResult<PagedResult<ActivityLogDto>>> GetMyActivity([FromQuery] PageQuery query)
        {
            return Ok(await _activityLogService.GetMineAsync(RequiredUserId, query));
        }
    }
}
