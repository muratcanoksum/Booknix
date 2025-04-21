using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [Route("Error/404")]
    public IActionResult Error404()
    {
        return View("Error404");
    }

    [Route("Error/{code:int}")]
    public IActionResult General(int code)
    {
        if (code == 404)
            return RedirectToAction("Error404");

        ViewData["Title"] = $"Hata Kodu: {code}";
        ViewBag.Error = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz.";
        ViewBag.Details = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error.ToString(); // detay

        return View("Error");
    }

    [Route("Error/AccessDenied")]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Erişim Reddedildi - 403";
        return View();
    }


    [Route("test-error")]
    public IActionResult ThrowError()
    {
        throw new Exception("Test hatası: Veritabanı bağlantısı başarısız oldu.");
    }

}
