using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Application.Services;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.MVCUI.Controllers
{
    [Auth]
    public class AccountController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IAuthService _authService;

        public AccountController(IProfileService profileService, IAuthService authService)
        {
            _profileService = profileService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return PartialView("_ProfilePartial", profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel dto)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);

            var success = await _profileService.UpdateProfileAsync(userId, dto);
            if (!success)
                return BadRequest("Güncelleme başarısız.");

            return Ok("Profil bilgileri başarıyla güncellendi.");
        }

        [HttpGet]
        public async Task<IActionResult> ProfilePhoto()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return PartialView("_ProfilePhotoPartial", profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilePhoto(IFormFile profileImage)
        {
            if (profileImage != null && profileImage.Length > 0)
            {
                var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);

                // Fotoğrafı kaydetmek için bir yol belirleyelim
                var directoryPath = Path.Combine("wwwroot", "images", "profile_images");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var filePath = Path.Combine(directoryPath, $"{userId}.jpg");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                var profileImagePath = $"/images/profile_images/{userId}.jpg";

                // Servis ile veritabanı güncelleme işlemi
                var success = await _profileService.UpdateProfileAsync(userId, new ProfileViewModel
                {
                    ProfileImagePath = profileImagePath
                });

                if (!success)
                    return BadRequest("Profil fotoğrafı güncellenemedi.");

                return Ok(profileImagePath); // Yeni fotoğraf yolunu döndür
            }

            return BadRequest("Bir dosya seçmelisiniz.");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return PartialView("_ChangePasswordPartial", profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                return BadRequest("Yeni şifreler eşleşmiyor.");
            }

            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);

            var success = await _authService.ResetPasswordWithPass(userId, oldPassword, newPassword);
            if (!success)
            {
                return BadRequest("Eski şifre yanlış veya şifre güncellenemedi.");
            }

            return Ok("Şifreniz başarıyla güncellendi.");
        }

    }
}
