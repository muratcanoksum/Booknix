using System;

namespace Booknix.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public Guid? AdminUserId { get; set; }
        public string Action { get; set; } = null!;
        public string? Entity { get; set; }      // Örn: "User", "Appointment"
        public string? EntityId { get; set; }    // Örn: "abc-123"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }

        // Navigation
        public User? AdminUser { get; set; }
    }
}