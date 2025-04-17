namespace Booknix.Domain.Entities.Enums
{
    public enum AppointmentStatus
    {
        Pending = 0,     // Randevu oluşturuldu, onay bekliyor
        Approved = 1,    // Randevu onaylandı
        Cancelled = 2,   // İptal edildi
        Completed = 3,   // Randevu başarıyla tamamlandı
        NoShow = 4       // Kullanıcı gelmedi
    }
}