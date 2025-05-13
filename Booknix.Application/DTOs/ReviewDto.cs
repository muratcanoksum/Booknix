using System;

namespace Booknix.Application.DTOs
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ServiceId { get; set; }
        
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public string UserName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
    }
} 