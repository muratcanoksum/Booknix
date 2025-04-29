using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class WorkerWorkingHourDto
    {
        public Guid WorkerId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; } = null;
        public TimeSpan? EndTime { get; set; } = null;
        public bool IsOnLeave { get; set; } = false; // İzinli mi (yıllık izin gibi)
        public bool IsDayOff { get; set; } = false; // Boş gün mü (hafta tatili gibi)
    }
}
