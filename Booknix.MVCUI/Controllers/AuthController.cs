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

        // LOGIN
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto dto, string? returnUrl)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
                return BadRequest("Email veya şifre hatalı.");

            if (result.Role == "Unverified")
                return BadRequest("Email adresiniz doğrulanmamış. Lütfen gelen kutunuzu kontrol ediniz.");

            HttpContext.Session.SetString("UserId", result.Id.ToString());
            HttpContext.Session.SetString("FullName", result.FullName);
            HttpContext.Session.SetString("Role", result.Role);
            HttpContext.Session.SetString("Email", result.Email);


            if (dto.RememberMe)
            {
                Response.Cookies.Append("RememberMe", "true", new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(1)
                });
            }
            
            if (result.Role == "Admin")
            {
                return Ok("/Admin");
            }

            return Ok(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }


        // REGISTER
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            var success = await _authService.RegisterAsync(dto, "Client");
            if (!success)
                return BadRequest("Email zaten kullanılıyor.");


            TempData["Success"] = "Kayıt başarılı! Lütfen email adresinizi kontrol edin ve doğrulama linkine tıklayın.";
            return Ok(); // AJAX success
        }

        // LOGOUT
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("RememberMe");

            return RedirectToAction("Login");
        }

        // VERIFY EMAIL
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var result = await _authService.VerifyEmailAsync(token);

            ViewBag.Status = result.Success ? "success" : "error";
            ViewBag.Message = result.Message;

            return View("VerifyResult"); // Basit bir sonuç sayfası gösterebiliriz
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            var success = await _authService.SendPasswordResetTokenAsync(dto.Email);
            if (!success)
                return BadRequest("Geçersiz e-posta adresi veya doğrulanmamış kullanıcı.");

            TempData["Success"] = "Eğer bu e-posta sistemde varsa, şifre sıfırlama bağlantısı gönderildi.";
            return Ok();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            var success = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!success)
                return BadRequest("Geçersiz veya süresi dolmuş bağlantı.");

            TempData["Success"] = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.";
            return Ok();
        }


    }
}
