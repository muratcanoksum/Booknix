using Booknix.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public async Task InvokeAsync(HttpContext context, IUserSessionRepository sessionRepo)
        {
            var sessionKey = context.Request.Cookies["PersistentSessionKey"];
            var userIdStr = context.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(sessionKey) && Guid.TryParse(userIdStr, out var userId))
            {
                var session = await sessionRepo.GetBySessionKeyAsync(userId, sessionKey);

                var ip = context.Connection.RemoteIpAddress?.ToString();
                var ua = context.Request.Headers["User-Agent"].ToString();

                if (session == null || !session.IsActive || session.IpAddress != ip || session.UserAgent != ua)
                {
                    context.Session.Clear();
                    context.Response.Cookies.Delete("PersistentSessionKey");
                    context.Response.Redirect("/Auth/Login?sessionExpired=true");
                    return;
                }

                await sessionRepo.UpdateLastAccessedAtAsync(userId, sessionKey);
            }

            await _next(context);
        }

    }
}
