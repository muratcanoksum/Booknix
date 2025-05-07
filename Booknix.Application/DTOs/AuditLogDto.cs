using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class AuditLogDto
    {
        public string Action { get; set; } = null!;
        public string Timestamp { get; set; } = null!;
        public string? IPAddress { get; set; }
        public string? Description { get; set; }
    }

}
