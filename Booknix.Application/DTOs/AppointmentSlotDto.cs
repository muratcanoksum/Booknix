using Booknix.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class AppointmentSlotDto
    {
        public DateTime Date { get; set; }           // 2025-05-01
        public TimeSpan Time { get; set; }           // 09:00
        public bool IsAvailable { get; set; }        // true / false
        public AppointmentStatus? Status { get; set; } // "Pending", "Approved", "Cancelled", "Completed", "NoShow"
    }

}
