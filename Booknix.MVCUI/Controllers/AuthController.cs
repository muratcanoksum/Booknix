using Microsoft.AspNetCore.Mvc;
using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Infrastructure.Email;
using Booknix.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Booknix.MVCUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly BooknixDbContext _context;
        private readonly IEmailSender _emailSender;

        public AuthController(
            IAuthService authService,
            BooknixDbContext context,
            IEmailSender emailSender)
        {
            _authService = authService;
            _context = context;
            _emailSender = emailSender;
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

            var (result, msg) = await _authService.LoginAsync(dto);
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString()
                            ?? "0.0.0.0";





            if (result == null)
                return BadRequest(msg);

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
                // Bu admin'e ait daha önce kayıtlı bir IP var mı?
                var existingIp = await _context.TrustedIps
                    .FirstOrDefaultAsync(x => x.UserId == result.Id && x.IpAddress == ipAddress);

                // Eğer hiç kayıt yoksa bile IP'yi ekle
                if (existingIp == null || !existingIp.IsApproved)
                {
                    if (existingIp == null)
                    {
                        _context.TrustedIps.Add(new TrustedIp
                        {
                            Id = Guid.NewGuid(),
                            UserId = result.Id,
                            IpAddress = ipAddress,
                            IsApproved = false,
                            RequestedAt = DateTime.UtcNow
                        });

                        await _context.SaveChangesAsync();
                    }

                    // Adminleri al (en azından bir tane olmalı: giriş yapan kişi)
                    var admins = await _context.Users
                        .Include(u => u.Role)
                        .Where(u => u.Role!.Name == "Admin" && u.IsEmailConfirmed)
                        .ToListAsync();

                    // Eğer hiç admin yoksa bile kendisine mail gönderilsin
                    if (!admins.Any())
                    {
                        admins.Add(new User
                        {
                            Email = result.Email,
                            FullName = result.FullName
                        });
                    }

                    // Mail hazırla ve gönder
                    var protocol = Request.IsHttps ? "https" : "http";
                    var approvalUrl = $"{protocol}://{Request.Host}/Auth/ApproveIp?userId={result.Id}&ip={ipAddress}";
                    var subject = $"🚨 Güvenlik Uyarısı: {result.FullName} yeni bir IP'den giriş yapıyor";
                    var htmlBody = $@"
            <p><strong>{result.FullName}</strong> adlı yönetici <strong>{ipAddress}</strong> IP adresinden panele erişmek istedi.</p>
            <p>Bu IP'yi onaylamak için <a href='{approvalUrl}'>buraya tıklayın</a>.</p>
            <p><small>Zaman: {DateTime.UtcNow:dd MMM yyyy HH:mm}</small></p>";

                    foreach (var admin in admins)
                    {
                        await _emailSender.SendEmailAsync(
                            to: admin.Email,
                            subject: subject,
                            htmlBody: htmlBody,
                            from: "Booknix Güvenlik Sistemi"
                        );
                    }

                    return BadRequest("Yeni bir IP adresinden giriş algılandı. Onay maili gönderildi.");
                }
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
        [HttpGet]
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


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!result.Success)
                return BadRequest(result.Message);

            TempData["Success"] = result.Message;
            return Ok();
        }


    }
}
