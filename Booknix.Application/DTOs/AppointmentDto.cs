using Booknix.Domain.Entities.Enums;
using System;

namespace Booknix.Application.DTOs
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string WorkerName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string ServiceName { get; set; } = string.Empty;
        public Guid ServiceId { get; set; }
        public int? ReviewRating { get; set; }

    }
} 