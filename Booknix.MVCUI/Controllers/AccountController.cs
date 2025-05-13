using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.MVCUI.Controllers
{
    [Auth]
    public class AccountController(
        IProfileService profileService,
        IAuthService authService,
        IAppointmentService appointmentService,
        ISecurityService securityService,
        IReviewService reviewService) : Controller
    {
        private readonly IProfileService _profileService = profileService;
        private readonly IAuthService _authService = authService;
        private readonly IAppointmentService _appointmentService = appointmentService;
        private readonly ISecurityService _securityService = securityService;
        private readonly IReviewService _reviewService = reviewService;

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> ProfileView()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return View("Profile/Profile", profile);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Profile()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return PartialView("Profile/Sections/_ProfilePartial", profile);
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
        [AjaxOnly]
        public async Task<IActionResult> ProfilePhoto()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return PartialView("Profile/Sections/_ProfilePhotoPartial", profile);
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
        [AjaxOnly]
        public async Task<IActionResult> ChangePassword()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var profile = await _profileService.GetProfileAsync(userId);
            return PartialView("Profile/Sections/_ChangePasswordPartial", profile);
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

            var result = await _authService.ChangePasswordAsync(userId, oldPassword, newPassword);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult ChangeEmail()
        {
            var userEmail = HttpContext.Session.GetString("Email")!;
            return PartialView("Profile/Sections/_ChangeEmailPartial", userEmail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(string newEmail)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var result = await _authService.ChangeEmail(userId, newEmail);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmailVerify(string VerificationCode)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var (result, email) = await _authService.ChangeEmailVerify(userId, VerificationCode);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            HttpContext.Session.SetString("Email", email!);
            return Ok(result.Message);
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Delete()
        {
            return PartialView("Profile/Sections/_DeletePartial");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string CurrentPassword)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var result = await _authService.DeleteAccount(userId, CurrentPassword);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVerify(string VerificationCode)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var result = await _authService.DeleteAccountVerify(userId, VerificationCode);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);

        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Appointments()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var appointments = await _appointmentService.GetUserAppointmentsAsync(userId);
            return PartialView("Appointments/_AppointmentsPartial", appointments);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> AppointmentDetail(Guid id)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var appointmentDetail = await _appointmentService.GetAppointmentDetailAsync(userId, id);

            if (appointmentDetail == null)
            {
                return NotFound("Randevu bulunamadı veya erişim izniniz yok.");
            }

            return PartialView("Appointments/_AppointmentDetailPartial", appointmentDetail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var success = await _appointmentService.CancelAppointmentAsync(userId, id);

            if (!success)
            {
                return BadRequest("Randevu iptal edilemedi.");
            }

            return Ok("Randevu başarıyla iptal edildi.");
        }

        // Security

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Security()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _securityService.GetSecurityDataAsync(Guid.Parse(userId));
            return PartialView("Security/Security", result);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> GetAuditLogs(int page = 1, int pageSize = 10)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var result = await _securityService.GetAuditLogsPagedAsync(userId, page, pageSize);

            return Json(result); // { logs: [...], currentPage: 1, totalPages: 5 }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReview(CreateReviewDto dto)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            dto.UserId = userId;

            var result = await _reviewService.CreateReviewAsync(dto);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message ?? "Değerlendirmeniz için teşekkür ederiz.");
        }



        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> GetReview(Guid serviceId)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId")!);
            var reviewResult = await _reviewService.GetUserReviewAsync(userId, serviceId);

            if (!reviewResult.IsSuccess || reviewResult.Value == null)
                return NoContent();

            return Json(reviewResult.Value);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> GetReviewByAppointment(Guid appointmentId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var result = await _reviewService.GetUserReviewByAppointmentIdAsync(userId, appointmentId);
            if (!result.IsSuccess)
            {
                return NoContent();
            }

            return Json(result.Value);
        }








    }
}
