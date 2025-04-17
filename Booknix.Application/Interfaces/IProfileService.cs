using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(Guid userId);
        Task<bool> UpdateProfileAsync(Guid userId, ProfileViewModel dto);
    }
}
