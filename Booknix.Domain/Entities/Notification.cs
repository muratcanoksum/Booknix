using Booknix.Domain.Entities.Enums;

namespace Booknix.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; } // Bildirimi kime göstereceðiz
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; } = NotificationType.Info; // Success, Error, Info gibi
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; } // Ne zaman okundu
        public bool IsDeleted { get; set; } = false; // Silinmiþ bildirimler için

    }
}
