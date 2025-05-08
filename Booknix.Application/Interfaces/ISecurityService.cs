using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface ISecurityService
    {
        Task<SecurityViewModel> GetSecurityDataAsync(Guid userId);
        Task<PagedAuditLogResult> GetAuditLogsPagedAsync(Guid userId, int page, int pageSize);

    }
}
