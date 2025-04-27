using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Booknix.Infrastructure.Filters
{
    public class LocationAdminAttribute : BaseAuthorizationFilter
    {
        protected override bool IsAuthorized(HttpContext context)
        {
            var roleStr = context.Session.GetString("LocationRole");

            if (string.IsNullOrEmpty(roleStr))
                return false;

            // LocationRole enum kontrolü
            if (!Enum.TryParse(roleStr, out Booknix.Domain.Entities.Enums.LocationRole role))
                return false;

            // Sadece LocationAdmin ise izin ver
            return role == Booknix.Domain.Entities.Enums.LocationRole.LocationAdmin;
        }

        protected override IActionResult GetUnauthorizedResult(HttpContext context)
        {
            return new RedirectToActionResult("AccessDenied", "Error", null);
        }
    }
}
