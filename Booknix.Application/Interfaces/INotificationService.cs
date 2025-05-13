using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;

namespace Booknix.Application.Interfaces
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task AddNotificationAsync(Guid userId, string title, string message, NotificationType type = NotificationType.Info);
        Task SoftDeleteAsync(Guid notificationId);
        Task SoftDeleteAllAsync(Guid userId);

    }
}
