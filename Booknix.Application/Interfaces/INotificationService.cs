using Booknix.Domain.Entities;

namespace Booknix.Application.Interfaces
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 20);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task AddNotificationAsync(Guid userId, string title, string message);
        Task SoftDeleteAsync(Guid notificationId);
        Task SoftDeleteAllAsync(Guid userId);

    }
}
