using System;

namespace Booknix.Application.DTOs
{
    public class UpdateReviewDto
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
} 