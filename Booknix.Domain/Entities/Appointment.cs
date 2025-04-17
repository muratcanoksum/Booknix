using System;

namespace Booknix.Domain.Entities
{

    public enum AppointmentStatus
    {
        Pending = 0,       // Oluþturuldu ama henüz onaylanmadý
        Approved = 1,      // Onaylandý
        Cancelled = 2,     // Kullanýcý ya da sistem tarafýndan iptal edildi
        Completed = 3,     // Randevu gerçekleþti
        NoShow = 4         // Kullanýcý gelmedi
    }


    public class Appointment
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; } // Müþteri
        public Guid ServiceId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        // Navigation
        public User? User { get; set; }
        public Service? Service { get; set; }
    }
    
}