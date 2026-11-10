using Microsoft.AspNetCore.Identity;
using CedarDelivery.Constants;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class RoleApplicationService : IRoleApplicationService
    {
        private readonly IRoleApplicationRepository _applicationRepository;
        private readonly IDriverProfileRepository _driverProfileRepository;
        private readonly UserManager<User> _userManager;
        private readonly INotificationService _notifications;
        private readonly IActivityLogService _activityLog;

        public RoleApplicationService(
            IRoleApplicationRepository applicationRepository,
            IDriverProfileRepository driverProfileRepository,
            UserManager<User> userManager,
            INotificationService notifications,
            IActivityLogService activityLog)
        {
            _applicationRepository = applicationRepository;
            _driverProfileRepository = driverProfileRepository;
            _userManager = userManager;
            _notifications = notifications;
            _activityLog = activityLog;
        }

        public async Task<RoleApplicationDto> ApplyAsync(string userId, ApplyForRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");

            var requestedRole = request.RequestedRole!.Value;
            var roleName = RoleName(requestedRole);

            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                throw new BadRequestException($"You are already a {roleName}.");
            }

            if (await _applicationRepository.HasPendingAsync(userId, requestedRole))
            {
                throw new BadRequestException("You already have a pending application for this role.");
            }

            // Drivers get a profile right away (not approved yet) holding their vehicle details.
            if (requestedRole == RequestedRole.Driver)
            {
                if (!request.VehicleType.HasValue)
                {
                    throw new BadRequestException("Choose your vehicle type.");
                }

                // Re-applying after a rejection updates the existing profile.
                var profile = await _driverProfileRepository.GetByUserIdAsync(userId);
                var isNew = profile == null;
                profile ??= new DriverProfile { UserId = userId, CreatedAt = DateTime.Now };

                profile.VehicleType = request.VehicleType.Value;
                profile.PlateNumber = request.PlateNumber?.Trim();
                profile.IsApproved = false;
                profile.IsOnline = false;

                if (isNew)
                {
                    await _driverProfileRepository.AddAsync(profile);
                }
                else
                {
                    await _driverProfileRepository.SaveAsync();
                }
            }

            var application = new RoleApplication
            {
                UserId = userId,
                User = user,
                RequestedRole = requestedRole,
                Details = request.Details.Trim(),
                Status = ApplicationStatus.Pending,
                CreatedAt = DateTime.Now
            };
            await _applicationRepository.AddAsync(application);

            foreach (var admin in await _userManager.GetUsersInRoleAsync(Roles.Admin))
            {
                await _notifications.NotifyAsync(admin.Id,
                    $"New {roleName} application",
                    $"{user.FirstName} {user.LastName} applied to become a {roleName}.",
                    NotificationType.General,
                    "/Admin/Applications");
            }
            await _activityLog.LogAsync(userId, ActivityType.ApplicationSubmitted, $"Applied to become a {roleName}.");

            return RoleApplicationDto.FromEntity(application);
        }

        public async Task<List<RoleApplicationDto>> GetMineAsync(string userId)
        {
            var applications = await _applicationRepository.GetByUserAsync(userId);
            return applications.Select(RoleApplicationDto.FromEntity).ToList();
        }

        public async Task<PagedResult<RoleApplicationDto>> GetApplicationsAsync(ApplicationQuery query)
        {
            var (items, totalCount) = await _applicationRepository.GetPagedAsync(query.Status, query.Page, query.PageSize);

            return new PagedResult<RoleApplicationDto>
            {
                Items = items.Select(RoleApplicationDto.FromEntity).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<RoleApplicationDto> ReviewAsync(int id, string adminId, bool approve, string? note)
        {
            var application = await _applicationRepository.GetWithUserAsync(id)
                ?? throw new NotFoundException("Application not found.");

            if (application.Status != ApplicationStatus.Pending)
            {
                throw new BadRequestException("This application was already reviewed.");
            }

            var roleName = RoleName(application.RequestedRole);

            application.Status = approve ? ApplicationStatus.Approved : ApplicationStatus.Rejected;
            application.ReviewedByUserId = adminId;
            application.ReviewedAt = DateTime.Now;
            application.ReviewNote = note?.Trim();

            if (approve)
            {
                if (!await _userManager.IsInRoleAsync(application.User, roleName))
                {
                    var roleResult = await _userManager.AddToRoleAsync(application.User, roleName);
                    if (!roleResult.Succeeded)
                    {
                        // Stop before saving "Approved": the application must not say approved
                        // if the user didn't actually get the role.
                        throw new BadRequestException("Could not grant the role: " +
                            string.Join(" ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                if (application.RequestedRole == RequestedRole.Driver)
                {
                    var profile = await _driverProfileRepository.GetByUserIdAsync(application.UserId);
                    if (profile == null)
                    {
                        // Should not happen (created when applying), but never leave a driver without a profile.
                        profile = new DriverProfile { UserId = application.UserId, CreatedAt = DateTime.Now };
                        await _driverProfileRepository.AddAsync(profile);
                    }
                    profile.IsApproved = true;
                }

                // Roles are stored in the login cookie. Changing the security stamp makes the
                // old cookie invalid, so the user signs in again and gets the new role.
                await _userManager.UpdateSecurityStampAsync(application.User);
            }

            await _applicationRepository.SaveAsync();

            if (approve)
            {
                await _notifications.NotifyAsync(application.UserId,
                    $"You're now a {roleName}!",
                    $"Your {roleName} application was approved. Sign out and back in to open your {roleName} tools.",
                    NotificationType.ApplicationApproved,
                    sendEmail: true);
            }
            else
            {
                await _notifications.NotifyAsync(application.UserId,
                    $"{roleName} application not approved",
                    string.IsNullOrWhiteSpace(note) ? $"Your {roleName} application was not approved." : $"Your {roleName} application was not approved: {note}",
                    NotificationType.ApplicationRejected,
                    sendEmail: true);
            }

            await _activityLog.LogAsync(adminId, ActivityType.ApplicationReviewed,
                $"{(approve ? "Approved" : "Rejected")} {roleName} application #{application.Id} of {application.User.Email}.");

            return RoleApplicationDto.FromEntity(application);
        }

        private static string RoleName(RequestedRole role) =>
            role == RequestedRole.Merchant ? Roles.Merchant : Roles.Driver;
    }
}
