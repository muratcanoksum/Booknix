using System;
using Booknix.Domain.Entities.Enums;

namespace Booknix.Domain.Entities
{

    public class Appointment
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid AppointmentSlotId { get; set; }

        public string? Notes { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Service? Service { get; set; }
        public AppointmentSlot? AppointmentSlot { get; set; }
        public Review? Review { get; set; } 
        
    }

}