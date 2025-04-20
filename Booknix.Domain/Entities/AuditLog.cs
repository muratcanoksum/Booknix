using System;

namespace Booknix.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public Guid? AdminUserId { get; set; }   // İşlemi yapan kullanıcı (null olabilir)
        public string Action { get; set; } = null!; // "Login", "Logout", "PasswordChange" gibi
        public string? Entity { get; set; }         // "User", "Appointment" vb.
        public string? EntityId { get; set; }       // Örn: kullanıcı ID'si
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }

        // 💬 Açıklayıcı bilgi eklemek için:
        public string? Description { get; set; }

        // Navigation
        public User? AdminUser { get; set; }
    }

}