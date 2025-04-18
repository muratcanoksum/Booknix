using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
namespace Booknix.Infrastructure.Filters;

public class AdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var role = context.HttpContext.Session.GetString("Role");
        var userId = context.HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId) || role != "Admin")
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
        }
    }
}