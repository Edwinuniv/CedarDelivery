using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;

namespace CedarDelivery.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IGenericRepository<ActivityLog> _activityLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ActivityLogService> _logger;

        public ActivityLogService(IGenericRepository<ActivityLog> activityLogRepository, IHttpContextAccessor httpContextAccessor, ILogger<ActivityLogService> logger)
        {
            _activityLogRepository = activityLogRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(string userId, ActivityType action, string? details = null)
        {
            // The activity log is extra information: if saving it fails,
            // the user's real action (placing an order, editing a store) must still succeed.
            try
            {
                var http = _httpContextAccessor.HttpContext;

                var log = new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Details = Truncate(details, 500),
                    IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Truncate(http?.Request.Headers.UserAgent.ToString(), 500),
                    CreatedAt = DateTime.Now
                };

                await _activityLogRepository.AddAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not save activity log {Action} for user {UserId}.", action, userId);
            }
        }

        // Cut text to the column's max length so the insert never fails.
        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }
            return value.Substring(0, maxLength);
        }
    }
}
