using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Infrastructure.Logging
{
    public class AuditLogger(BooknixDbContext context, IHttpContextAccessor httpContextAccessor) : IAuditLogger
    {
        private readonly BooknixDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task LogAsync(
            Guid? userId = null,
            string? action = null,
            string? sourcePage = null,
            Guid? adminUserId = null,
            string? sessionKey = null,
            string? ipAddress = null,
            string? description = null)
        {
            var context = _httpContextAccessor.HttpContext;

            // IP adresi
            var ip = ipAddress ?? context?.Connection.RemoteIpAddress?.ToString();

            // SessionKey (cookie'den otomatik çekilir)
            if (string.IsNullOrEmpty(sessionKey))
            {
                sessionKey = context?.Request.Cookies["PersistentSessionKey"];
            }

            // Kullanıcı kontrolü
            Guid? validUserId = null;
            if (userId != Guid.Empty && await _context.Users.AnyAsync(u => u.Id == userId))
            {
                validUserId = userId;
            }

            // Admin kullanıcı kontrolü
            Guid? validAdminUserId = null;
            if (adminUserId.HasValue && await _context.Users.AnyAsync(u => u.Id == adminUserId.Value))
            {
                validAdminUserId = adminUserId;
            }

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = validUserId,
                AdminUserId = validAdminUserId,
                Action = action ?? "",
                SourcePage = sourcePage,
                SessionKey = sessionKey,
                IPAddress = ip,
                Timestamp = DateTime.UtcNow,
                Description = description ?? $"{action} işlemi gerçekleştirildi."
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
