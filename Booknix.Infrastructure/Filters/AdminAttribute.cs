using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.Infrastructure.Filters;

public class AdminAttribute : BaseAuthorizationFilter
{
    protected override bool IsAuthorized(HttpContext context)
    {
        var userId = context.Session.GetString("UserId");
        var role = context.Session.GetString("Role");
        return !string.IsNullOrEmpty(userId) && role == "Admin";
    }

    protected override IActionResult GetUnauthorizedResult(HttpContext context)
    {
        return new RedirectToActionResult("AccessDenied", "Error", null);
    }

}
