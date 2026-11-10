using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Controllers
{
    // Admin pages: dashboard, store approvals, applications, users, all orders, activity log.
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;
        private readonly IStoreService _storeService;
        private readonly IRoleApplicationService _applicationService;
        private readonly IOrderService _orderService;
        private readonly IActivityLogService _activityLogService;

        public AdminController(UserManager<User> userManager, IUserRepository userRepository,
            IAdminService adminService, IStoreService storeService, IRoleApplicationService applicationService,
            IOrderService orderService, IActivityLogService activityLogService)
            : base(userManager, userRepository)
        {
            _adminService = adminService;
            _storeService = storeService;
            _applicationService = applicationService;
            _orderService = orderService;
            _activityLogService = activityLogService;
        }

        // GET /Admin?days=30
        public async Task<IActionResult> Index([FromQuery] AnalyticsQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new AnalyticsQuery();
            }
            return View(await _adminService.GetAnalyticsAsync(query));
        }

        // ---------- Stores ----------

        public async Task<IActionResult> Stores()
        {
            return View(await _storeService.GetPendingStoresAsync());
        }

        [HttpPost]
        public async Task<IActionResult> SetStoreApproval(int id, bool isApproved, string? returnUrl)
        {
            await TryAsync(() => _storeService.SetApprovalAsync(id, RequiredUserId, isApproved),
                isApproved ? "Store approved." : "Store suspended.");
            return RedirectToLocal(returnUrl, nameof(Stores));
        }

        // ---------- Applications ----------

        public async Task<IActionResult> Applications([FromQuery] ApplicationQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new ApplicationQuery();
            }
            // Default view: what still needs a decision.
            if (!Request.Query.ContainsKey("status"))
            {
                query.Status = ApplicationStatus.Pending;
            }
            query.PageSize = 20;
            ViewBag.Status = query.Status;
            return View(await _applicationService.GetApplicationsAsync(query));
        }

        [HttpPost]
        public async Task<IActionResult> ReviewApplication(int id, bool approve, string? note)
        {
            await TryAsync(() => _applicationService.ReviewAsync(id, RequiredUserId, approve, note),
                approve ? "Application approved." : "Application rejected.");
            return RedirectToAction(nameof(Applications));
        }

        // ---------- Users ----------

        public async Task<IActionResult> Users([FromQuery] UserQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new UserQuery();
            }
            query.PageSize = 20;
            ViewBag.Query = query;
            return View(await _adminService.GetUsersAsync(query));
        }

        [HttpPost]
        public async Task<IActionResult> SetUserActive(string id, bool isActive, string? returnUrl)
        {
            await TryAsync(() => _adminService.SetUserActiveAsync(id, RequiredUserId, isActive),
                isActive ? "User activated." : "User deactivated.");
            return RedirectToLocal(returnUrl, nameof(Users));
        }

        // ---------- Orders and activity ----------

        public async Task<IActionResult> Orders([FromQuery] OrderQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new OrderQuery();
            }
            query.PageSize = 20;
            ViewBag.Query = query;
            return View(await _orderService.GetAllOrdersAsync(query));
        }

        public async Task<IActionResult> Activity([FromQuery] ActivityQuery query)
        {
            if (!ModelState.IsValid)
            {
                query = new ActivityQuery();
            }
            query.PageSize = 30;
            ViewBag.Query = query;
            return View(await _activityLogService.GetAllAsync(query));
        }
    }
}
