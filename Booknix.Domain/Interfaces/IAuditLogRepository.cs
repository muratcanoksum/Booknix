using Booknix.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetLogsByEntityIdAsync(string entityId);
    }

}
