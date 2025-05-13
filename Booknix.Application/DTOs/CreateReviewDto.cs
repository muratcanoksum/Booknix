using System;

namespace Booknix.Application.DTOs
{
    public class CreateReviewDto
    {
        public Guid UserId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid AppointmentId { get; set; }
        
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
} 