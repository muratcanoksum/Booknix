using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.Infrastructure.Filters
{
    public class WorkerAttribute : BaseAuthorizationFilter
    {
        protected override bool IsAuthorized(HttpContext context)
        {
            var roleStr = context.Session.GetString("LocationRole");

            if (string.IsNullOrEmpty(roleStr))
                return false;

            // LocationRole enum kontrolü
            if (!Enum.TryParse(roleStr, out Booknix.Domain.Entities.Enums.LocationRole role))
                return false;

            // Sadece LocationEmployee ise izin ver
            return role == Booknix.Domain.Entities.Enums.LocationRole.LocationEmployee;
        }

        protected override IActionResult GetUnauthorizedResult(HttpContext context)
        {
            return new RedirectToActionResult("AccessDenied", "Error", null);
        }
    }
} 