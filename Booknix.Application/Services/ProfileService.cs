using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserProfileRepository _profileRepo;

        public ProfileService(IUserProfileRepository profileRepo)
        {
            _profileRepo = profileRepo;
        }

        public async Task<ProfileViewModel?> GetProfileAsync(Guid userId)
        {
            var profile = await _profileRepo.GetByUserIdAsync(userId);
            if (profile == null) return null;

            return new ProfileViewModel
            {
                PhoneNumber = profile.PhoneNumber,
                BirthDate = profile.BirthDate,
                ProfileImagePath = profile.ProfileImagePath
            };
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, ProfileViewModel dto)
        {
            var profile = await _profileRepo.GetByUserIdAsync(userId);
            if (profile == null) return false;

            profile.PhoneNumber = dto.PhoneNumber;
            profile.BirthDate = dto.BirthDate;
            profile.ProfileImagePath = dto.ProfileImagePath;

            await _profileRepo.UpdateAsync(profile);
            await _profileRepo.SaveChangesAsync();
            return true;
        }
    }
}
