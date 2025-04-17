namespace Booknix.Domain.Entities.Enums
{
    public enum SlotStatus
    {
        Available = 0,   // Slot açık ve uygun
        Booked = 1,      // Bir kullanıcı tarafından alındı
        Cancelled = 2    // Slot iptal edildi (çalışan/sistem)
    }
}