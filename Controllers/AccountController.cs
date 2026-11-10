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
    // The user's own account: profile, merchant/driver applications, activity history.
    // (Login, register and passwords are the Identity pages in Areas/Identity.)
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly IProfileService _profileService;
        private readonly IRoleApplicationService _applicationService;
        private readonly IActivityLogService _activityLogService;

        public AccountController(UserManager<User> userManager, IUserRepository userRepository,
            IProfileService profileService, IRoleApplicationService applicationService, IActivityLogService activityLogService)
            : base(userManager, userRepository)
        {
            _profileService = profileService;
            _applicationService = applicationService;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Profile()
        {
            ViewBag.Email = User.Identity?.Name;
            return View(await _profileService.GetAsync(RequiredUserId));
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Email = User.Identity?.Name;
                return View(request);
            }

            await TryAsync(() => _profileService.UpdateAsync(RequiredUserId, request), "Profile saved.");
            return RedirectToAction(nameof(Profile));
        }

        public async Task<IActionResult> Apply()
        {
            return View(new ApplyPageViewModel
            {
                Applications = await _applicationService.GetMineAsync(RequiredUserId)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Apply([Bind(Prefix = "Request")] ApplyForRoleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(new ApplyPageViewModel
                {
                    Applications = await _applicationService.GetMineAsync(RequiredUserId),
                    Request = request
                });
            }

            await TryAsync(() => _applicationService.ApplyAsync(RequiredUserId, request),
                "Application sent! An admin will review it soon.");
            return RedirectToAction(nameof(Apply));
        }

        public async Task<IActionResult> Activity([FromQuery] PageQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new PageQuery();
            }
            query.PageSize = 20;
            return View(await _activityLogService.GetMineAsync(RequiredUserId, query));
        }
    }
}
