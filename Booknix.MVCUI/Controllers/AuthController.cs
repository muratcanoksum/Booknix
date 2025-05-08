using Microsoft.AspNetCore.Mvc;
using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Persistence.Data;
using Booknix.Infrastructure.Filters;

namespace Booknix.MVCUI.Controllers;

public class AuthController(
    IAuthService authService,
    BooknixDbContext context,
    IEmailSender emailSender) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly BooknixDbContext _context = context;
    private readonly IEmailSender _emailSender = emailSender;

    // LOGIN
    [UnAuth]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Full URL'yi alıp hash kısmını ekleyelim
        var fullUrl = Request.Path + Request.QueryString;  // Path + QueryString kısmı



        Console.WriteLine(fullUrl);
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [UnAuth]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto dto, string? returnUrl)
    {
        var (result, msg) = await _authService.LoginAsync(dto);

        if (result == null)
            return BadRequest(msg);

        HttpContext.Session.SetString("UserId", result.Id.ToString());
        HttpContext.Session.SetString("FullName", result.FullName);
        HttpContext.Session.SetString("Role", result.Role);
        HttpContext.Session.SetString("Email", result.Email);
        HttpContext.Session.SetString("LocationId", result.LocationId?.ToString() ?? "");
        HttpContext.Session.SetString("LocationRole", result.LocationRole?.ToString() ?? "");
        HttpContext.Session.SetString("SessionKey", result.SessionKey);



        if (dto.RememberMe)
        {
            Response.Cookies.Append("RememberMe", "true", new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(1)
            });
        }

        return Ok(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
    }

    // REGISTER
    [UnAuth]
    [HttpGet]
    public IActionResult Register() => View();

    [UnAuth]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        var success = await _authService.RegisterAsync(dto, "Client");
        if (!success)
            return BadRequest("Email zaten kullanılıyor.");


        TempData["Success"] = "Kayıt başarılı! Lütfen email adresinizi kontrol edin ve doğrulama linkine tıklayın.";
        return Ok(); // AJAX success
    }

    [Auth]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        var sessionKey = HttpContext.Session.GetString("SessionKey");
        var userIdStr = HttpContext.Session.GetString("UserId");

        if (!string.IsNullOrEmpty(sessionKey) && Guid.TryParse(userIdStr, out var userId))
        {
            await _authService.LogoutAsync(userId, sessionKey);
        }

        HttpContext.Session.Clear();
        Response.Cookies.Delete("PersistentSessionKey");
        Response.Cookies.Delete("RememberMe");

        return RedirectToAction("Login");
    }



    // VERIFY EMAIL
    [UnAuth]
    [HttpGet]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        var result = await _authService.VerifyEmailAsync(token);

        ViewBag.Status = result.Success ? "success" : "error";
        ViewBag.Message = result.Message;

        return View("VerifyResult"); // Basit bir sonuç sayfası gösterebiliriz
    }

    [UnAuth]
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [UnAuth]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
    {
        var success = await _authService.SendPasswordResetTokenAsync(dto.Email);
        if (!success)
            return BadRequest("Geçersiz e-posta adresi veya doğrulanmamış kullanıcı.");

        TempData["Success"] = "Eğer bu e-posta sistemde varsa, şifre sıfırlama bağlantısı gönderildi.";
        return Ok();
    }

    [UnAuth]
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
        var expired = await _authService.CheckTokenExpire(token);
        if (!expired)
        {
            ViewBag.Error = "Geçersiz veya süresi dolmuş bağlantı.";
            return View("Error");
        }
        ViewBag.Token = token;
        return View();
    }

    [UnAuth]
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
        if (!result.Success)
            return BadRequest(result.Message);

        TempData["Success"] = result.Message;
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ApproveIp(string token)
    {
        var model = await _authService.GetIpApprovalDetailsAsync(token);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveIpAjax(string AdminEmail, string AdminPassword, string Token)
    {
        var result = await _authService.ApproveIpAsync(Token, AdminEmail, AdminPassword);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }


}
