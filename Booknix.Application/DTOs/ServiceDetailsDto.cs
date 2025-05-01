using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class ServiceDetailsDto
    {
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }

        public string? LocationSlug { get; set; }
        public string? LocationName { get; set; }

        public List<WorkerMiniDto>? Workers { get; set; }
    }

}
