using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class SecurityService(IUserSessionRepository sessionRepo, IAuditLogRepository auditRepo) : ISecurityService
    {
        private readonly IUserSessionRepository _sessionRepo = sessionRepo;
        private readonly IAuditLogRepository _auditRepo = auditRepo;

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
            var all = await _auditRepo.GetLogsByUserIdAsync(userId);

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
