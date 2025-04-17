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
        return View("Error");
    }
}
