using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Persistence.Data;
using Microsoft.AspNetCore.Http;

namespace Booknix.Infrastructure.Logging
{
    public class AuditLogger : IAuditLogger
    {
        private readonly BooknixDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(BooknixDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(Guid? adminUserId, string action, string entity, string? entityId = null, string? ipAddress = null, string? description = null)
        {
            var ip = ipAddress ?? _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            // Varsayılan açıklamalar
            if (string.IsNullOrEmpty(description))
            {
                description = action switch
                {
                    "Login" => "Kullanıcı başarılı şekilde giriş yaptı.",
                    "Logout" => "Kullanıcı sistemden çıkış yaptı.",
                    "FailedLogin" => "Başarısız giriş denemesi yapıldı.",
                    "PasswordChange" => "Şifre başarıyla değiştirildi.",
                    "PasswordReset" => "Şifre sıfırlama işlemi tamamlandı.",
                    "PasswordResetRequest" => "Şifre sıfırlama isteği gönderildi.",
                    "EmailChange" => "E-posta adresi değiştirildi.",
                    "EmailVerified" => "E-posta doğrulandı.",
                    "UnverifiedLoginAttempt" => "Doğrulanmamış e-posta ile giriş denemesi yapıldı.",
                    "AccountDeleted" => "Kullanıcı hesabı silindi.",
                    _ => $"{action} işlemi gerçekleştirildi."
                };
            }

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                AdminUserId = adminUserId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                IPAddress = ip,
                Description = description
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

    }
}
