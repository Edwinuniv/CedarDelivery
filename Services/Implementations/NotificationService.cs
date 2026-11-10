using CedarDelivery.Models;
using CedarDelivery.Repositories.Interfaces;
using CedarDelivery.Services.Interfaces;
using CedarDelivery.ViewModels.Api;

namespace CedarDelivery.Services.Implementations
{
    // All notifications go through NotifyAsync, so live push (SignalR, Phase 5)
    // and email (Phase 6) can be added here in one place.
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task NotifyAsync(string userId, string title, string message, NotificationType type, string? redirectUrl = null)
        {
            try
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RedirectUrl = redirectUrl,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not save notification \"{Title}\" for user {UserId}.", title, userId);
            }
        }

        public async Task<PagedResult<NotificationDto>> GetMineAsync(string userId, PageQuery query)
        {
            var (items, totalCount) = await _notificationRepository.GetByUserPagedAsync(userId, query.Page, query.PageSize);

            return new PagedResult<NotificationDto>
            {
                Items = items.Select(NotificationDto.FromEntity).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.CountUnreadAsync(userId);
        }

        public async Task MarkReadAsync(int id, string userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);

            // Someone else's notification looks the same as a missing one.
            if (notification == null || notification.UserId != userId)
            {
                throw new NotFoundException("Notification not found.");
            }

            notification.IsRead = true;
            await _notificationRepository.SaveAsync();
        }

        public async Task MarkAllReadAsync(string userId)
        {
            await _notificationRepository.MarkAllReadAsync(userId);
        }
    }
}
