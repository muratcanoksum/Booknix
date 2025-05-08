using System;
using System.Threading.Tasks;

namespace Booknix.Application.Interfaces
{
    public interface IAuditLogger
    {
        Task LogAsync(
            Guid? userId = null,
            string? action = null,
            string? sourcePage = null,
            Guid? adminUserId = null,
            string? sessionKey = null,
            string? ipAddress = null,
            string? description = null);
    }
}
