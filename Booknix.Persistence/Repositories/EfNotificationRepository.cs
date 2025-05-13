using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfNotificationRepository : INotificationRepository
    {
        private readonly BooknixDbContext _context;

        public EfNotificationRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 20)
        {
            return await _context.Notifications
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(x => x.UserId == userId && x.ReadAt == null && !x.IsDeleted)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null && notification.ReadAt == null)
            {
                notification.ReadAt = DateTime.UtcNow;
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(x => x.UserId == userId && x.ReadAt == null)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.ReadAt = DateTime.UtcNow;
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null && !notification.IsDeleted)
            {
                notification.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAllAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }

    }
}
