using System.ComponentModel.DataAnnotations;

namespace Booknix.Domain.Entities.Enums
{
    public enum AppointmentStatus
    {
        [Display(Name = "Onay Bekliyor")]
        Pending = 0,     // Randevu oluşturuldu, onay bekliyor

        [Display(Name = "Onaylandı")]
        Approved = 1,    // Randevu onaylandı

        [Display(Name = "İptal Edildi")]
        Cancelled = 2,   // İptal edildi

        [Display(Name = "Tamamlandı")]
        Completed = 3,   // Randevu başarıyla tamamlandı

        [Display(Name = "Gelmedi")]
        NoShow = 4       // Kullanıcı gelmedi
    }
}