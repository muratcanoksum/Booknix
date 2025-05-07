using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Application.DTOs
{
    public class PagedAuditLogResult
    {
        public List<AuditLogDto> Logs { get; set; } = [];
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }


}
