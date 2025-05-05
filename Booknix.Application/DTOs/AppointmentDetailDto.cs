using System;

namespace Booknix.Application.DTOs
{
    public class AppointmentDetailDto
    {
        public Guid Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string LocationAddress { get; set; } = string.Empty;
        public string LocationPhone { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string WorkerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool CanCancel { get; set; }
    }
} 