using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(Guid userId);
        Task AddAsync(Notification notification);
        Task MarkAsReadAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
