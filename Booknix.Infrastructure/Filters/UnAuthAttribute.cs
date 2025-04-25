using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.Infrastructure.Filters
{
    public class UnAuthAttribute : BaseAuthorizationFilter
    {
        protected override bool IsAuthorized(HttpContext context)
        {
            // Kullanıcı giriş yapmamışsa erişime izin ver, giriş yapmışsa engelle
            return string.IsNullOrEmpty(context.Session.GetString("UserId"));
        }

        protected override IActionResult GetUnauthorizedResult(HttpContext context)
        {
            // Kullanıcı zaten giriş yapmışsa, ana sayfaya yönlendir
            return new RedirectToActionResult("Index", "Home", null);
        }
    }
}
