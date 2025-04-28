namespace Booknix.Domain.Entities;

public class WorkerWorkingHour
{
    public Guid Id { get; set; }

    public Guid WorkerId { get; set; }
    public Worker Worker { get; set; } = null!;

    public DateTime Date { get; set; } // sadece gün (YYYY-MM-DD)

    public TimeSpan? StartTime { get; set; } // Nullable! (çalışmadığı günlerde boş olabilir)
    public TimeSpan? EndTime { get; set; }   // Nullable! (çalışmadığı günlerde boş olabilir)

    public bool IsOnLeave { get; set; }  // İzinli mi (yıllık izin gibi)
    public bool IsDayOff { get; set; }    // Boş gün mü (hafta tatili gibi)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
