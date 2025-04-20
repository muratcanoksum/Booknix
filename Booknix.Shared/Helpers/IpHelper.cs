using Microsoft.AspNetCore.Http;

namespace Booknix.Shared.Helpers
{
    public static class IpHelper
    {
        public static string GetCurrentIp(IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString()
                ?? "0.0.0.0";
        }
    }
}
