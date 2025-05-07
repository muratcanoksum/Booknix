using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class ProfileService(IUserProfileRepository profileRepo, IUserSessionRepository sessionRepo, IAuditLogRepository auditRepo) : IProfileService
    {
        private readonly IUserProfileRepository _profileRepo = profileRepo;
        private readonly IUserSessionRepository _sessionRepo = sessionRepo;
        private readonly IAuditLogRepository _auditRepo = auditRepo;

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

        // security 

        public async Task<SecurityViewModel> GetSecurityDataAsync(Guid userId)
        {
            var sessions = await _sessionRepo.GetActiveSessionsByUserIdAsync(userId);
            var pagedLogs = await GetAuditLogsPagedAsync(userId, page: 1, pageSize: 10);

            return new SecurityViewModel
            {
                Sessions = sessions,
                InitialAuditLogs = pagedLogs
            };
        }



        public async Task<PagedAuditLogResult> GetAuditLogsPagedAsync(Guid userId, int page, int pageSize)
        {
            var all = await _auditRepo.GetLogsByEntityIdAsync(userId.ToString());

            var total = all.Count;
            var logs = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AuditLogDto
                {
                    Action = x.Action,
                    Timestamp = x.Timestamp.ToLocalTime().ToString("dd.MM.yyyy HH:mm"),
                    IPAddress = x.IPAddress,
                    Description = x.Description
                })
                .ToList();

            return new PagedAuditLogResult
            {
                Logs = logs,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }



    }
}
