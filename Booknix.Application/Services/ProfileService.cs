﻿using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class ProfileService(IUserProfileRepository profileRepo) : IProfileService
    {
        private readonly IUserProfileRepository _profileRepo = profileRepo;

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

            // Profil Fotoğrafı Güncelleniyor
            if (!string.IsNullOrEmpty(dto.ProfileImagePath))
            {
                profile.ProfileImagePath = dto.ProfileImagePath;
            }

            // Eğer telefon numarası var, o zaman güncelle
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                profile.PhoneNumber = dto.PhoneNumber;
            }

            // Eğer doğum tarihi varsa, o zaman güncelle
            if (dto.BirthDate.HasValue)
            {
                profile.BirthDate = DateTime.SpecifyKind(dto.BirthDate.Value, DateTimeKind.Utc);
            }

            await _profileRepo.UpdateAsync(profile);
            await _profileRepo.SaveChangesAsync();
            return true;
        }

    }
}
