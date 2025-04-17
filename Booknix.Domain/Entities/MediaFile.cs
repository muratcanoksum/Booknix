using System;

namespace Booknix.Domain.Entities
{
    public class MediaFile
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Bağlantılar
        public Guid? LocationId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ServiceId { get; set; }

        // Navigation
        public Location? Location { get; set; }
        public User? User { get; set; }
        public Service? Service { get; set; }
    }
}