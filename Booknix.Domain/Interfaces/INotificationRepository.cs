using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task AddNotificationAsync(Notification notification);
        Task SoftDeleteAsync(Guid notificationId);
        Task SoftDeleteAllAsync(Guid userId);
    }
}
