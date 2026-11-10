using CedarDelivery.Models;

namespace CedarDelivery.Services.Interfaces
{
    public interface IActivityLogService
    {
        // Records what a user did, with their IP address and browser.
        Task LogAsync(string userId, ActivityType action, string? details = null);
    }
}
