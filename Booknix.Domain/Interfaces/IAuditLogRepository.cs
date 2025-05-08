using Booknix.Domain.Entities;


namespace Booknix.Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetLogsByUserIdAsync(Guid userId);
    }

}
