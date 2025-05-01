using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class AppointmentSlotPageDto
    {
        public List<AppointmentSlotDto> Slots { get; set; } = new();
        public string LocationName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public string WorkerName { get; set; } = string.Empty;
    }


}
