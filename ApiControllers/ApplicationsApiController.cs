using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.ApiControllers
{
    // /api/applications: become a Merchant or Driver (any user), review applications (Admin).
    [Route("api/applications")]
    [Authorize]
    public class ApplicationsApiController : BaseApiController
    {
        private readonly IRoleApplicationService _applicationService;

        public ApplicationsApiController(IRoleApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // POST /api/applications   { "requestedRole": "Driver", "details": "...", "vehicleType": "Motorcycle" }
        [HttpPost]
        public async Task<ActionResult<RoleApplicationDto>> Apply(ApplyForRoleRequest request)
        {
            var application = await _applicationService.ApplyAsync(RequiredUserId, request);
            return StatusCode(StatusCodes.Status201Created, application);
        }

        // GET /api/applications/mine
        [HttpGet("mine")]
        public async Task<ActionResult<List<RoleApplicationDto>>> GetMine()
        {
            return Ok(await _applicationService.GetMineAsync(RequiredUserId));
        }

        // GET /api/applications?status=Pending&page=1
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<PagedResult<RoleApplicationDto>>> GetApplications([FromQuery] ApplicationQuery query)
        {
            return Ok(await _applicationService.GetApplicationsAsync(query));
        }

        // PATCH /api/applications/4/review   { "approve": true, "note": "Welcome!" }
        [HttpPatch("{id:int}/review")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<RoleApplicationDto>> Review(int id, ReviewApplicationRequest request)
        {
            return Ok(await _applicationService.ReviewAsync(id, RequiredUserId, request.Approve!.Value, request.Note));
        }
    }
}
