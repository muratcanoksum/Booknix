using Booknix.Application.Interfaces;
using Booknix.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Booknix.Infrastructure.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserSessionRepository sessionRepo, IAuditLogger auditLogger)
        {
            var sessionKey = context.Request.Cookies["PersistentSessionKey"];
            var userIdStr = context.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(sessionKey) && Guid.TryParse(userIdStr, out var userId))
            {
                var session = await sessionRepo.GetBySessionKeyAsync(userId, sessionKey);
                var ip = context.Connection.RemoteIpAddress?.ToString();
                var ua = context.Request.Headers["User-Agent"].ToString();

                if (session == null ||
                    !session.IsActive ||
                    session.ExpiresAt < DateTime.UtcNow ||
                    session.IpAddress != ip ||
                    session.UserAgent != ua)
                {
                    // Session expire olduysa işaretle
                    if (session != null && session.ExpiresAt < DateTime.UtcNow)
                    {
                        session.IsActive = false;
                        await sessionRepo.DeactivateBySessionKeyAsync(userId, sessionKey);

                        await auditLogger.LogAsync(
                            userId,
                            "SessionExpired",
                            "Middleware",
                            null,
                            sessionKey,
                            ip,
                            "Oturum süresi dolduğu için sonlandırıldı."
                        );
                    }
                    else
                    {
                        await auditLogger.LogAsync(
                            userId,
                            "SessionRejected",
                            "Middleware",
                            null,
                            sessionKey,
                            ip,
                            "Oturum geçersiz ya da farklı cihaz/IP ile uyumsuzluk nedeniyle iptal edildi."
                        );
                    }

                    context.Session.Clear();
                    context.Response.Cookies.Delete("PersistentSessionKey");
                    context.Response.Cookies.Delete("RememberMe");

                    var currentUrl = context.Request.Path + context.Request.QueryString;
                    context.Response.Redirect($"/Auth/Login?sessionExpired=true");
                    return;
                }

                await sessionRepo.UpdateLastAccessedAtAsync(userId, sessionKey);
            }

            await _next(context);
        }
    }
}
