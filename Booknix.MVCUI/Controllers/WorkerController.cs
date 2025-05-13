using Booknix.Application.Interfaces;
using Booknix.Domain.Entities.Enums;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.MVCUI.Controllers
{
    [Worker]
    public class WorkerController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IWorkerService _workerService;
        private readonly IReviewService _reviewService;

        public WorkerController(
            IAppointmentService appointmentService,
            IWorkerService workerService,
            IReviewService reviewService)
        {
            _appointmentService = appointmentService;
            _workerService = workerService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("UserId") ?? Guid.Empty.ToString());
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Auth");
            }

            var worker = await _workerService.GetWorkerByUserIdAsync(userId);
            if (worker == null)
            {
                return NotFound("Çalışan bilgileri bulunamadı.");
            }

            var appointments = await _appointmentService.GetWorkerAppointmentsAsync(worker.Id);
            
            ViewBag.Worker = worker;
            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(Guid appointmentId, string status)
        {
            try
            {
                var userId = Guid.Parse(HttpContext.Session.GetString("UserId") ?? Guid.Empty.ToString());
                if (userId == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
                }

                var worker = await _workerService.GetWorkerByUserIdAsync(userId);
                if (worker == null)
                {
                    return BadRequest(new { success = false, message = "Çalışan bilgileri bulunamadı." });
                }

                // Parse the status string to appointment status enum
                if (!Enum.TryParse<AppointmentStatus>(status, out var appointmentStatus))
                {
                    return BadRequest(new { success = false, message = "Geçersiz randevu durumu." });
                }

                // Call service to update the appointment status
                var result = await _workerService.UpdateAppointmentStatusAsync(worker.Id, appointmentId, appointmentStatus);
                if (!result)
                {
                    return BadRequest(new { success = false, message = "Randevu durumu güncellenemedi." });
                }

                TempData["SuccessMessage"] = "Randevu durumu başarıyla güncellendi.";
                return Ok(new { success = true, message = "Randevu durumu başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"İşlem sırasında bir hata oluştu: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentReview(Guid appointmentId)
        {
            try
            {
                var userId = Guid.Parse(HttpContext.Session.GetString("UserId") ?? Guid.Empty.ToString());
                if (userId == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "Kullanıcı oturumu bulunamadı." });
                }

                var worker = await _workerService.GetWorkerByUserIdAsync(userId);
                if (worker == null)
                {
                    return BadRequest(new { success = false, message = "Çalışan bilgileri bulunamadı." });
                }

                // Randevu değerlendirmesini service üzerinden al
                var review = await _reviewService.GetReviewByAppointmentIdAsync(appointmentId);
                if (review == null)
                {
                    return NotFound(new { success = false, message = "Değerlendirme bulunamadı." });
                }

                // Başarılı sonuç döndür
                return Ok(new { 
                    success = true, 
                    data = review,
                    message = "Değerlendirme başarıyla getirildi."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"İşlem sırasında bir hata oluştu: {ex.Message}" });
            }
        }
    }
} 