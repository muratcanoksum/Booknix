using Booknix.Application.Interfaces;
using Booknix.Application.Services;
using Booknix.Application.DTOs;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Booknix.MVCUI.Controllers
{
    [LocationAdmin]
    public class LocationAdminController(ILocationAdminService locationAdminService) : Controller
    {
        private readonly ILocationAdminService _locationAdminService = locationAdminService;

        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            var id = HttpContext.Session.GetString("LocationId");

            var location = await _locationAdminService.GetLocationByIdAsync(Guid.Parse(id!));

            TempData["Title"] = location?.Name;

            return View(location);
        }

        // SERVICE OPERATIONS
        [HttpGet("/LocationAdmin/GetServicesByLocation/{locationId}")]
        public async Task<IActionResult> GetServicesByLocation(Guid locationId)
        {
            var model = await _locationAdminService.GetServicesByLocationAsync(locationId);
            return PartialView("Location/LocationModules/Service/ServiceListPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LocationAdmin/Service/Add")]
        public async Task<IActionResult> AddServiceToLocation(ServiceCreateDto dto)
        {
            if (!await _locationAdminService.LocationExistsAsync(dto.LocationId))
            {
                return BadRequest("Lokasyon bulunamadı. Lütfen geçerli bir lokasyon seçiniz.");
            }

            var result = await _locationAdminService.AddServiceToLocationAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpGet]
        [Route("LocationAdmin/Service/Get/{id}")]
        public async Task<IActionResult> GetServiceById(Guid id)
        {
            var service = await _locationAdminService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound("Servis bulunamadı.");
            }

            return PartialView("Location/LocationModules/Service/ServiceDetailsPartial", service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LocationAdmin/Service/Update")]
        public async Task<IActionResult> UpdateService(ServiceUpdateDto dto)
        {
            if (!await _locationAdminService.LocationExistsAsync(dto.LocationId))
            {
                return BadRequest("Lokasyon bulunamadı. Lütfen geçerli bir lokasyon seçiniz.");
            }

            var result = await _locationAdminService.UpdateServiceAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LocationAdmin/Service/Delete/{id}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var result = await _locationAdminService.DeleteServiceAsync(id);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LocationAdmin/Service/RemoveWorker")]
        public async Task<IActionResult> RemoveWorkerFromService(Guid workerId, Guid serviceId)
        {
            try
            {
                var serviceEmployee = await _locationAdminService.GetServiceEmployeeAsync(serviceId, workerId);

                if (serviceEmployee == null)
                    return BadRequest("Çalışan bu servise atanmamış.");

                var result = await _locationAdminService.RemoveWorkerFromServiceAsync(serviceEmployee.Id);

                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"İşlem sırasında bir hata oluştu: {ex.Message}");
            }
        }
        
        // WORKER OPERATIONS
        [HttpGet("/LocationAdmin/GetWorkersByLocation/{locationId}")]
        public async Task<IActionResult> GetWorkersByLocation(Guid locationId)
        {
            var workers = await _locationAdminService.GetAllWorkersAsync(locationId);
            ViewBag.LocationId = locationId;
            return PartialView("Location/LocationModules/Worker/PartialView", workers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LocationAdmin/Worker/Add")]
        public async Task<IActionResult> AddWorkerToLocation(WorkerAddDto dto)
        {
            var result = await _locationAdminService.AddWorkerToLocationAsync(dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LocationAdmin/Worker/Edit")]
        public async Task<IActionResult> EditWorkerToLocation(Guid Id, WorkerAddDto dto)
        {
            var result = await _locationAdminService.UpdateWorkerAsync(Id, dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LocationAdmin/Worker/Delete")]
        public async Task<IActionResult> DeleteWorker(Guid id)
        {
            // Get the location admin's user ID directly from the session
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId))
            {
                return BadRequest("Lokasyon bilgisi bulunamadı.");
            }
            
            // Get admin ID from session if possible, or use empty GUID - the service will only check
            // if the worker being deleted is the admin themselves
            var adminUserId = HttpContext.Session.GetString("UserId");
            var adminId = !string.IsNullOrEmpty(adminUserId) ? Guid.Parse(adminUserId) : Guid.Empty;
            
            var result = await _locationAdminService.DeleteWorkerAsync(id, adminId);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("LocationAdmin/Worker/UpdateEmail")]
        public async Task<IActionResult> UpdateWorkerEmail(Guid workerId, string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
            {
                return BadRequest("E-posta adresi boş olamaz.");
            }
            
            var result = await _locationAdminService.UpdateWorkerEmailAsync(workerId, newEmail);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        // WORKER HOURS OPERATIONS
        [HttpGet]
        [Route("/LocationAdmin/GetWorkingHoursByLocation/{locationId}")]
        public async Task<IActionResult> GetWorkingHoursByLocation(Guid locationId)
        {
            var workers = await _locationAdminService.GetAllWorkersAsync(locationId);
            ViewBag.LocationId = locationId;
            return PartialView("Location/LocationModules/WorkerHour/WorkerHourMainPartial", workers);
        }

        [HttpGet]
        [Route("/LocationAdmin/GetWorkerWorkingHours/{workerId}/{year}/{month}")]
        public async Task<IActionResult> GetWorkerWorkingHours(Guid workerId, int year, int month)
        {
            var workingHours = await _locationAdminService.GetWorkerWorkingHoursAsync(workerId, year, month);

            var result = workingHours.Select(x => new
            {
                Day = x.Date.Day,
                IsOnLeave = x.IsOnLeave,
                IsDayOff = x.IsDayOff,
                StartTime = x.StartTime?.ToString(@"hh\:mm"),
                EndTime = x.EndTime?.ToString(@"hh\:mm")
            });

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LocationAdmin/WorkerHour/Add")]
        public async Task<IActionResult> AddWorkerHour(WorkerWorkingHourDto dto)
        {
            var result = await _locationAdminService.AddWorkerWorkingHourAsync(dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }
    }
}
