using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, ProfileViewModel dto);
        Task<SecurityViewModel> GetSecurityDataAsync(Guid userId);
        Task<PagedAuditLogResult> GetAuditLogsPagedAsync(Guid userId, int page, int pageSize);

    }
}
