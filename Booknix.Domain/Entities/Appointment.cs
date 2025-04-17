using System;

namespace Booknix.Domain.Entities
{

    public enum AppointmentStatus
    {
        Pending = 0,       // Olu�turuldu ama hen�z onaylanmad�
        Approved = 1,      // Onayland�
        Cancelled = 2,     // Kullan�c� ya da sistem taraf�ndan iptal edildi
        Completed = 3,     // Randevu ger�ekle�ti
        NoShow = 4         // Kullan�c� gelmedi
    }


    public class Appointment
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; } // M��teri
        public Guid ServiceId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string? Notes { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        // Navigation
        public User? User { get; set; }
        public Service? Service { get; set; }
    }
    
}