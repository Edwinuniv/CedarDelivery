using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Interfaces
{
    // The signed-in user's own profile.
    public interface IProfileService
    {
        Task<ProfileRequest> GetAsync(string userId);
        Task UpdateAsync(string userId, ProfileRequest request);
    }
}
