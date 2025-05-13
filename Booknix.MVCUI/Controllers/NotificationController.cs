using Booknix.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Booknix.Infrastructure.Filters;

namespace Booknix.MVCUI.Controllers
{
    [Auth]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationController(INotificationService notificationService, IHttpContextAccessor httpContextAccessor)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Json(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid notificationId)
        {
            await _notificationService.SoftDeleteAsync(notificationId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            await _notificationService.SoftDeleteAllAsync(userId);
            return Ok();
        }
    }
}
