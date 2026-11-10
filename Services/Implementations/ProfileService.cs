using Microsoft.AspNetCore.Identity;
using CedarDelivery.Models;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> _userManager;
        private readonly IActivityLogService _activityLog;

        public ProfileService(UserManager<User> userManager, IActivityLogService activityLog)
        {
            _userManager = userManager;
            _activityLog = activityLog;
        }

        public async Task<ProfileRequest> GetAsync(string userId)
        {
            var user = await GetUserAsync(userId);
            return new ProfileRequest
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task UpdateAsync(string userId, ProfileRequest request)
        {
            var user = await GetUserAsync(userId);

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            // UserManager validates the user and saves it (Identity owns the AspNetUsers table).
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new BadRequestException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _activityLog.LogAsync(userId, ActivityType.ProfileUpdated, "Updated profile.");
        }

        private async Task<User> GetUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");
        }
    }
}
