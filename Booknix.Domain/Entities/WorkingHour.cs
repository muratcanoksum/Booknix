using System;

namespace Booknix.Domain.Entities
{
    public class WorkingHour
    {
        public Guid Id { get; set; }

        public Guid LocationId { get; set; }      // Hangi mekana ait
        public DayOfWeek DayOfWeek { get; set; }  // Pazartesi, Salı, vs.

        public TimeSpan OpenTime { get; set; }    // Örn: 09:00
        public TimeSpan CloseTime { get; set; }   // Örn: 18:00

        // Navigation
        public Location? Location { get; set; }
    }
}