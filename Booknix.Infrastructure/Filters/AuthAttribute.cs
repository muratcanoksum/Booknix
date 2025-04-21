using Microsoft.AspNetCore.Http;

namespace Booknix.Infrastructure.Filters;

public class AuthAttribute : BaseAuthorizationFilter
{
    protected override bool IsAuthorized(HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Session.GetString("UserId"));
    }
}
