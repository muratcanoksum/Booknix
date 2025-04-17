using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.MVCUI.Controllers
{
    [Auth]
    public class AccountController : Controller
    {
        private readonly IProfileService _profileService;

        public AccountController(IProfileService profileService)
        {
            _profileService = profileService;
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




    }
}
