using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Infrastructure.Interfaces;

namespace Booknix.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly INotificationDispatcher _notificationDispatcher;

        public NotificationService(INotificationRepository notificationRepo, INotificationDispatcher notificationDispatcher)
        {
            _notificationRepo = notificationRepo;
            _notificationDispatcher = notificationDispatcher;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 20)
        {
            return await _notificationRepo.GetUserNotificationsAsync(userId, limit);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepo.GetUnreadCountAsync(userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            await _notificationRepo.MarkAsReadAsync(notificationId);
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _notificationRepo.MarkAllAsReadAsync(userId);
        }

        public async Task SoftDeleteAsync(Guid notificationId)
        {
            await _notificationRepo.SoftDeleteAsync(notificationId);
        }

        public async Task SoftDeleteAllAsync(Guid userId)
        {
            await _notificationRepo.SoftDeleteAllAsync(userId);
        }


        public async Task AddNotificationAsync(Guid userId, string title, string message)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepo.AddNotificationAsync(notification);

            // Gerçek zamanlı bildirim gönderelim
            await _notificationDispatcher.PushToUserAsync(userId, "notificationReceived", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                CreatedAt = notification.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}
