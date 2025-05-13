using System;

namespace Booknix.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid AppointmentId { get; set; }

        public int Rating { get; set; }  // 1–5 arası
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public Service? Service { get; set; }
    }
}