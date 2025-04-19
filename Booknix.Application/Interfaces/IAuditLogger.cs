using System;
using System.Threading.Tasks;

namespace Booknix.Application.Interfaces
{
    public interface IAuditLogger
    {
        Task LogAsync(Guid? adminUserId, string action, string entity, string? entityId = null, string? ipAddress = null, string? description = null);
    }
}
