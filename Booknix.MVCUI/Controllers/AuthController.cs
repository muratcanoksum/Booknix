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
            {
                TempData["Error"] = "Email veya şifre hatalı.";
                return View();
            }

            // TODO: Session veya Cookie ile kullanıcıyı oturumda tut
            TempData["Success"] = $"Hoşgeldin {result.FullName}!";
            return RedirectToAction("Index", "Home");
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
    }
}
