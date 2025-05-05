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

        public WorkerController(
            IAppointmentService appointmentService,
            IWorkerService workerService)
        {
            _appointmentService = appointmentService;
            _workerService = workerService;
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
                    return BadRequest("Kullanıcı oturumu bulunamadı.");
                }

                var worker = await _workerService.GetWorkerByUserIdAsync(userId);
                if (worker == null)
                {
                    return BadRequest("Çalışan bilgileri bulunamadı.");
                }

                // Parse the status string to appointment status enum
                if (!Enum.TryParse<AppointmentStatus>(status, out var appointmentStatus))
                {
                    return BadRequest("Geçersiz randevu durumu.");
                }

                // Call service to update the appointment status
                var result = await _workerService.UpdateAppointmentStatusAsync(worker.Id, appointmentId, appointmentStatus);
                if (!result)
                {
                    return BadRequest("Randevu durumu güncellenemedi.");
                }

                TempData["SuccessMessage"] = "Randevu durumu başarıyla güncellendi.";
                return Ok("Randevu durumu başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return BadRequest($"İşlem sırasında bir hata oluştu: {ex.Message}");
            }
        }
    }
} 