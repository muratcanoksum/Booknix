using Microsoft.AspNetCore.Mvc;
using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;

namespace Booknix.MVCUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return BadRequest("Hatalı giriş.");

            HttpContext.Session.SetString("FullName", result.FullName);
            HttpContext.Session.SetString("Role", result.Role);
            HttpContext.Session.SetString("Email", result.Email);

            // Süreyi cookie üzerinden belirleyeceğiz (Session süresi zaten tanımlı)
            if (dto.RememberMe)
            {
                // 2 gün boyunca tarayıcıyı kapatsan bile session cookie silinmez
                Response.Cookies.Append("RememberMe", "true", new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(1)
                });
            }

            return Ok();
        }


        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var success = await _authService.RegisterAsync(dto, "Client");
            if (!success)
            {
                TempData["Error"] = "Email zaten kullanılıyor.";
                return View();
            }

            TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("RememberMe");

            return RedirectToAction("Login");
        }
    }
}
