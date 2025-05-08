namespace Booknix.Domain.Entities
{
    public class UserSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string SessionKey { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } // ✅ Yeni alan
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}
