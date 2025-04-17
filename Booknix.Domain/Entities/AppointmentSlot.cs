using System;
using Booknix.Domain.Entities.Enums;

namespace Booknix.Domain.Entities

{

    
    public class AppointmentSlot
    {
        public Guid Id { get; set; }

        public Guid LocationId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid? AssignedEmployeeId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public SlotStatus Status { get; set; } = SlotStatus.Available;

        // Navigation
        public Location? Location { get; set; }
        public Service? Service { get; set; }
        public User? AssignedEmployee { get; set; }
        public Appointment? Appointment { get; set; }
    }
}