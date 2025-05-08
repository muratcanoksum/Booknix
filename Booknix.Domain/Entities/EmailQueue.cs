using Booknix.Domain.Entities.Enums;


namespace Booknix.Domain.Entities
{
    public class EmailQueue
    {
        public Guid Id { get; set; }
        public string To { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string From { get; set; } = "Booknix";

        public EmailQueueStatus Status { get; set; } = EmailQueueStatus.Pending;
        public int TryCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
