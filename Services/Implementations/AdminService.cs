using Microsoft.AspNetCore.Identity;
using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IDriverProfileRepository _driverProfileRepository;
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly UserManager<User> _userManager;
        private readonly IActivityLogService _activityLog;

        public AdminService(
            IUserRepository userRepository,
            IStoreRepository storeRepository,
            IDriverProfileRepository driverProfileRepository,
            IAnalyticsRepository analyticsRepository,
            UserManager<User> userManager,
            IActivityLogService activityLog)
        {
            _userRepository = userRepository;
            _storeRepository = storeRepository;
            _driverProfileRepository = driverProfileRepository;
            _analyticsRepository = analyticsRepository;
            _userManager = userManager;
            _activityLog = activityLog;
        }

        public async Task<PagedResult<UserDto>> GetUsersAsync(UserQuery query)
        {
            var (users, totalCount) = await _userRepository.GetPagedAsync(query.Search, query.Role, query.Page, query.PageSize);
            var roles = await _userRepository.GetRolesForUsersAsync(users.Select(u => u.Id));

            return new PagedResult<UserDto>
            {
                Items = users.Select(u => ToDto(u, roles.GetValueOrDefault(u.Id) ?? new List<string>())).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UserDto> GetUserAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");
            var roles = await _userRepository.GetRolesForUsersAsync(new[] { user.Id });
            return ToDto(user, roles.GetValueOrDefault(user.Id) ?? new List<string>());
        }

        public async Task<UserDto> SetUserActiveAsync(string userId, string adminId, bool isActive)
        {
            if (userId == adminId)
            {
                throw new BadRequestException("You can't deactivate your own account.");
            }

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");

            user.IsActive = isActive;

            if (!isActive)
            {
                // A deactivated driver stops receiving jobs; a deactivated merchant's stores close.
                var profile = await _driverProfileRepository.GetByUserIdAsync(userId);
                if (profile != null)
                {
                    profile.IsOnline = false;
                }

                foreach (var store in await _storeRepository.GetByOwnerAsync(userId))
                {
                    store.IsOpen = false;
                }

                // Makes their login cookie invalid, so they are signed out
                // at the next security check (see SecurityStampValidatorOptions in Program.cs).
                await _userManager.UpdateSecurityStampAsync(user);
            }

            // UpdateAsync saves the user and the tracked store/driver changes together.
            await _userManager.UpdateAsync(user);
            await _activityLog.LogAsync(adminId, ActivityType.UserStatusChanged,
                $"{(isActive ? "Activated" : "Deactivated")} user {user.Email}.");

            return await GetUserAsync(userId);
        }

        public async Task<AnalyticsDto> GetAnalyticsAsync(AnalyticsQuery query)
        {
            // "Last 30 days" = today plus the 29 days before it.
            var since = DateTime.Today.AddDays(-(query.Days - 1));
            return await _analyticsRepository.GetDashboardAsync(since, query.Days);
        }

        private static UserDto ToDto(User user, List<string> roles) => new UserDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = roles,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
