using System.ComponentModel.DataAnnotations;

namespace Booknix.Application.DTOs
{
    public class WorkerWorkingHourDto
    {
        [Required]
        public Guid WorkerId { get; set; }

        public DateTime Date { get; set; }

        public string? SelectedDays { get; set; } // <<< ARTIK STRING TUTACAĞIZ

        public TimeSpan? StartTime { get; set; } = null;
        public TimeSpan? EndTime { get; set; } = null;
        public bool IsOnLeave { get; set; } = false;
        public bool IsDayOff { get; set; } = false;
    }
}
