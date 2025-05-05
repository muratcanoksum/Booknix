using Booknix.Application.Interfaces;
using Booknix.Application.Services;
using Booknix.Application.DTOs;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.MVCUI.Controllers
{
    [LocationAdmin]
    [Route("panel/location")]
    public class LocationAdminController(ILocationService locationService) : Controller
    {
        private readonly ILocationService _locationService = locationService;


        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            var id = HttpContext.Session.GetString("LocationId");

            var location = await _locationService.GetLocationByIdAsync(Guid.Parse(id!));

            TempData["Title"] = location?.Name;
            TempData["BaseUrl"] = "/LocationAdmin";

            return View("~/Views/Location/details.cshtml", location);
        }

        // SERVICE OPERATIONS
        [AjaxOnly]
        [HttpGet("/LocationAdmin/GetServicesByLocation/{locationId}")]
        public async Task<IActionResult> GetServicesByLocation(Guid locationId)
        {
            var model = await _locationService.GetServicesByLocationAsync(locationId);
            return PartialView("~/Views/Location/Sections/Service/PartialView.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LocationAdmin/Service/Add")]
        public async Task<IActionResult> AddServiceToLocation(ServiceCreateDto dto)
        {
            if (!await _locationService.LocationExistsAsync(dto.LocationId))
            {
                return BadRequest("Lokasyon bulunamadı. Lütfen geçerli bir lokasyon seçiniz.");
            }

            var result = await _locationService.AddServiceToLocationAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpGet]
        [Route("/LocationAdmin/Service/Get/{id}")]
        public async Task<IActionResult> GetServiceById(Guid id)
        {
            var service = await _locationService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound("Servis bulunamadı.");
            }

            return PartialView("~/Views/Location/Sections/Service/_PartialHelper.cshtml", service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LocationAdmin/Service/Update")]
        public async Task<IActionResult> UpdateService(ServiceUpdateDto dto)
        {
            if (!await _locationService.LocationExistsAsync(dto.LocationId))
            {
                return BadRequest("Lokasyon bulunamadı. Lütfen geçerli bir lokasyon seçiniz.");
            }

            var result = await _locationService.UpdateServiceAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LocationAdmin/Service/Delete/{id}")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var result = await _locationService.DeleteServiceAsync(id);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LocationAdmin/Service/RemoveWorker")]
        public async Task<IActionResult> RemoveWorkerFromService(Guid workerId, Guid serviceId)
        {
            try
            {
                var serviceEmployee = await _locationService.GetServiceEmployeeAsync(serviceId, workerId);

                if (serviceEmployee == null)
                    return BadRequest("Çalışan bu servise atanmamış.");

                var result = await _locationService.RemoveWorkerFromServiceAsync(serviceEmployee.Id);

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
        [AjaxOnly]
        [HttpGet("/LocationAdmin/GetWorkersByLocation/{locationId}")]
        public async Task<IActionResult> GetWorkersByLocation(Guid locationId)
        {
            var workers = await _locationService.GetAllWorkersAsync(locationId);
            ViewBag.LocationId = locationId;
            return PartialView("~/Views/Location/Sections/Worker/PartialView.cshtml", workers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LocationAdmin/Worker/Add")]
        public async Task<IActionResult> AddWorkerToLocation(WorkerAddDto dto)
        {
            var result = await _locationService.AddWorkerToLocationAsync(dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LocationAdmin/Worker/Edit")]
        public async Task<IActionResult> EditWorkerToLocation(Guid Id, WorkerAddDto dto)
        {
            var result = await _locationService.UpdateWorkerAsync(Id, dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/LocationAdmin/Worker/Delete")]
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

            var result = await _locationService.DeleteWorkerAsync(id, adminId);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        // WORKER HOURS OPERATIONS
        [HttpGet]
        [Route("/LocationAdmin/GetWorkingHoursByLocation/{locationId}")]
        [AjaxOnly]
        public async Task<IActionResult> GetWorkingHoursByLocation(Guid locationId)
        {
            var workers = await _locationService.GetAllWorkersAsync(locationId);
            ViewBag.LocationId = locationId;
            return PartialView("~/Views/Location/Sections/WorkerHour/WorkerHourMainPartial.cshtml", workers);
        }

        [HttpGet]
        [AjaxOnly]
        [Route("/LocationAdmin/GetWorkerWorkingHours/{workerId}/{year}/{month}")]
        public async Task<IActionResult> GetWorkerWorkingHours(Guid workerId, int year, int month)
        {
            var workingHours = await _locationService.GetWorkerWorkingHoursAsync(workerId, year, month);

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
            var result = await _locationService.AddWorkerWorkingHourAsync(dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }
    }
}
