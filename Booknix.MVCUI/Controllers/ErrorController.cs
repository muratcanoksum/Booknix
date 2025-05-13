using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [Route("Error/404")]
    [HttpGet]
    public IActionResult Error404()
    {
        return View("Error404");
    }

    [Route("Error/{code:int}")]
    [HttpGet]
    public IActionResult General(int code)
    {
        var requestedUrl = HttpContext.Request.Query["url"]; // URL parametresini al
        if (code == 404)
            return RedirectToAction("Error404");

        ViewData["Title"] = $"Hata Kodu: {code}";
        ViewBag.Error = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz.";
        ViewBag.Details = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error.ToString(); // Hata detayları
        ViewBag.RequestedUrl = requestedUrl; // Hata yapılan URL'yi View'a geçir

        return View("Error");
    }

    [Route("Error/AccessDenied")]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Erişim Reddedildi - 403";
        return View();
    }
}

