using System;

namespace Booknix.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        public Guid? AdminUserId { get; set; }
        public User? AdminUser { get; set; }

        public string Action { get; set; } = null!;
        public string? SourcePage { get; set; }
        public string? SessionKey { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }

        public string? Description { get; set; }
    }
}
