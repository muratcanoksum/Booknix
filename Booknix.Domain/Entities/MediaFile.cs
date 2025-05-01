using System;

namespace Booknix.Domain.Entities
{
    public class MediaFile
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;  // Bu alanda hem dosya yolu hem de URL tutabiliriz
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Bağlantılar
        public Guid? LocationId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? SectorId { get; set; }

        // Navigation
        public Location? Location { get; set; }
        public User? User { get; set; }
        public Service? Service { get; set; }
        public Sector? Sector { get; set; }

        // Yöntem: Dosya yolu mı URL mi olduğunu kontrol et
        public bool IsUrl()
        {
            return Uri.TryCreate(FilePath, UriKind.Absolute, out Uri? uriResult) && uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
        }
    }
}
