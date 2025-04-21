using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Booknix.Infrastructure.Filters;

public abstract class BaseAuthorizationFilter : Attribute, IAuthorizationFilter
{
    protected abstract bool IsAuthorized(HttpContext context);

    protected virtual IActionResult GetUnauthorizedResult(HttpContext context)
    {
        var returnUrl = context.Request.Path + context.Request.QueryString;
        return new RedirectToActionResult("Login", "Auth", new { returnUrl });
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!IsAuthorized(context.HttpContext))
        {
            context.Result = GetUnauthorizedResult(context.HttpContext);
        }
    }
}
