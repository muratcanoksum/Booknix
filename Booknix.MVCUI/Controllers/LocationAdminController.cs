using Booknix.Application.Interfaces;
using Booknix.Application.Services;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;
using Booknix.Application.ViewModels;
using Booknix.Application.DTOs;

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

        [HttpGet]
        [Route("LocationAdmin/Services")]
        public async Task<IActionResult> GetServicesByLocation()
        {
            var id = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var model = await _locationAdminService.GetServiceCreateViewModelAsync(Guid.Parse(id));
            return PartialView("Service/ServiceListPartial", model);
        }

        [HttpGet]
        [Route("LocationAdmin/Service/Get/{id}")]
        public async Task<IActionResult> GetService(Guid id)
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            var service = await _locationAdminService.GetServiceByIdAsync(id);
            return PartialView("Service/ServiceDetailPartial", service);
        }

        [HttpPost]
        [Route("LocationAdmin/Service/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService([FromForm] ServiceCreateDto model)
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            model.LocationId = Guid.Parse(locationId);
            var result = await _locationAdminService.AddServiceAsync(model);

            if (result.Success)
                return Ok(result.Message);

            return BadRequest(result.Message);
        }

        [HttpPost]
        [Route("LocationAdmin/Service/Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateService([FromForm] ServiceUpdateDto model)
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            model.LocationId = Guid.Parse(locationId);
            var result = await _locationAdminService.UpdateServiceAsync(model);

            if (result.Success)
                return Ok(result.Message);

            return BadRequest(result.Message);
        }

        [HttpPost]
        [Route("LocationAdmin/Service/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            var result = await _locationAdminService.DeleteServiceAsync(id);

            if (result.Success)
                return Ok(result.Message);

            return BadRequest(result.Message);
        }

        [HttpPost]
        [Route("LocationAdmin/Service/RemoveWorker/{serviceId}/{workerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveWorkerFromService(Guid serviceId, Guid workerId)
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            var result = await _locationAdminService.RemoveWorkerFromServiceAsync(serviceId, workerId);

            if (result.Success)
                return Ok(result.Message);

            return BadRequest(result.Message);
        }

        [HttpGet]
        [Route("LocationAdmin/Workers")]
        public async Task<IActionResult> GetWorkersByLocation()
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            var workers = await _locationAdminService.GetWorkersByLocationAsync(Guid.Parse(locationId));
            ViewBag.LocationId = locationId;
            ViewBag.CurrentWorkerId = HttpContext.Session.GetString("WorkerId");
            return PartialView("Worker/WorkerListPartial", workers);
        }

        [HttpPost]
        [Route("LocationAdmin/Worker/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWorker([FromForm] WorkerAddDto dto)
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            dto.LocationId = Guid.Parse(locationId);
            var result = await _locationAdminService.AddWorkerAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [Route("LocationAdmin/Worker/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWorker([FromForm] WorkerUpdateDto dto)
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            dto.LocationId = Guid.Parse(locationId);
            var result = await _locationAdminService.UpdateWorkerAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPost]
        [Route("LocationAdmin/Worker/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorker(Guid id)
        {
            var locationId = HttpContext.Session.GetString("LocationId");
            if (string.IsNullOrEmpty(locationId)) return Unauthorized();

            // Kendini silmeye çalışıyor mu kontrol et
            var workerId = HttpContext.Session.GetString("WorkerId");
            if (!string.IsNullOrEmpty(workerId) && workerId == id.ToString())
            {
                return BadRequest("Kendinizi silemezsiniz.");
            }

            var result = await _locationAdminService.DeleteWorkerAsync(id);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }
    }
}
