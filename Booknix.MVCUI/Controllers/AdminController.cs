using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Booknix.Infrastructure.Filters;
using Booknix.Application.Interfaces;
using Booknix.Application.DTOs;

namespace Booknix.MVCUI.Controllers;

[Admin]
public class AdminController(IAdminService adminService) : Controller
{
    private readonly IAdminService _adminService = adminService;

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // SECTOR OPERATIONS

    [HttpGet]
    public async Task<IActionResult> Sector()
    {
        var sectors = await _adminService.GetAllSectorsAsync();
        return View("Sector/Index", sectors);
    }

    [HttpGet("Admin/Sector/Create")]
    public IActionResult CreateSector()
    {
        return View("Sector/Create");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSector(string Name)
    {
        var result = await _adminService.AddSectorAsync(Name);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        TempData["SectorInfo"] = result.Message;
        return Ok();
    }

    [HttpGet("Admin/Sector/Edit/{id}")]
    public async Task<IActionResult> EditSector(Guid id)
    {
        var sector = await _adminService.GetSectorByIdAsync(id);

        if (sector == null)
        {
            return NotFound("Sektör bulunamadı.");
        }

        return View("Sector/Edit", sector);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSector(Guid Id, String Name)
    {
        var result = await _adminService.UpdateSectorAsync(Id, Name);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        TempData["SectorInfo"] = result.Message;
        return Ok();
    }

    [HttpPost("Admin/Sector/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSector(Guid id)
    {
        var result = await _adminService.DeleteSectorAsync(id);
        if (!result.Success)
        {
            return BadRequest(result.Message);
        }
        TempData["SectorInfo"] = result.Message;
        return Ok();
    }

    // LOCATION OPERATIONS

    [HttpGet]
    public async Task<IActionResult> Location()
    {
        var locations = await _adminService.GetAllLocationsAsync();
        return View("Location/Index", locations);
    }

    [HttpGet("Admin/Location/Create")]
    public async Task<IActionResult> CreateLocation()
    {
        var sectors = await _adminService.GetAllSectorsAsync();

        var model = new LocationCreateDto
        {
            Sectors = sectors.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList()
        };

        return View("Location/Create", model);
    }

    [HttpPost()]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLocation(LocationCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Tüm alanların eksiksiz doldurulduğundan emin olun.");
        }

        var result = await _adminService.AddLocationAsync(dto.Name, dto.Address, dto.PhoneNumber, dto.SectorId);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        TempData["LocationInfo"] = result.Message;
        return Ok();
    }



    [HttpGet("Admin/Location/Edit/{id}")]
    public async Task<IActionResult> EditLocation(Guid id)
    {
        var location = await _adminService.GetLocationByIdAsync(id);
        if (location == null)
        {
            return NotFound("Lokasyon bulunamadı.");
        }

        var sectors = await _adminService.GetAllSectorsAsync(); // Drop-down için
        ViewBag.Sectors = sectors.ToList();

        return View("Location/Edit", location);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLocation(Guid Id, string Name, string Address, string PhoneNumber, Guid SectorId)
    {
        var result = await _adminService.UpdateLocationAsync(Id, Name, Address, PhoneNumber, SectorId);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        TempData["LocationInfo"] = result.Message;
        return Ok();
    }


    [HttpPost("Admin/Location/Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLocation(Guid id)
    {
        var result = await _adminService.DeleteLocationAsync(id);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        TempData["SectorInfo"] = result.Message;
        return Ok();
    }


    // LOCATION DETAİLS OPERATIONS



    [HttpGet("Admin/Location/Details/{id}")]
    public async Task<IActionResult> LocationDetails(Guid id)
    {
        var location = await _adminService.GetLocationByIdAsync(id);
        if (location == null)
            return NotFound("Lokasyon bulunamadı.");

        TempData["BaseUrl"] = "/Admin/Location";

        return View("/Views/Location/details.cshtml", location);
    }


    // SERVICE OPERATIONS

    [HttpGet("/Admin/Location/GetServicesByLocation/{locationId}")]
    [AjaxOnly]
    public async Task<IActionResult> GetServicesByLocation(Guid locationId)
    {
        var model = await _adminService.GetServicesByLocationAsync(locationId);
        return PartialView("~/Views/Location/Sections/Service/PartialView.cshtml", model);
    }




    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Location/Service/Add")]
    public async Task<IActionResult> AddServiceToLocation(ServiceCreateDto dto)
    {
        if (!await _adminService.LocationExistsAsync(dto.LocationId))
        {
            return BadRequest("Lokasyon bulunamadı. Lütfen geçerli bir lokasyon seçiniz.");
        }

        var result = await _adminService.AddServiceToLocationAsync(dto);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }

    [HttpGet]
    [AjaxOnly]
    [Route("Admin/Location/Service/Get/{id}")]
    public async Task<IActionResult> GetServiceById(Guid id)
    {
        var service = await _adminService.GetServiceByIdAsync(id);
        if (service == null)
        {
            return NotFound("Servis bulunamadı.");
        }

        return PartialView("~/Views/Location/Sections/Service/_PartialHelper.cshtml", service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Location/Service/Update")]
    public async Task<IActionResult> UpdateService(ServiceUpdateDto dto)
    {
        if (!await _adminService.LocationExistsAsync(dto.LocationId))
        {
            return BadRequest("Lokasyon bulunamadı. Lütfen geçerli bir lokasyon seçiniz.");
        }

        var result = await _adminService.UpdateServiceAsync(dto);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Location/Service/Delete/{id}")]
    public async Task<IActionResult> DeleteService(Guid id)
    {
        var result = await _adminService.DeleteServiceAsync(id);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Location/Service/RemoveWorker")]
    public async Task<IActionResult> RemoveWorkerFromService(Guid workerId, Guid serviceId)
    {
        // Bu action işçiyi servisten kaldıracak ama işçi kaydı silinmeyecek
        // Sadece ServicesEmployees tablosundaki kayıt silinecek

        try
        {
            var serviceEmployee = await _adminService.GetServiceEmployeeAsync(serviceId, workerId);

            if (serviceEmployee == null)
                return BadRequest("Çalışan bu servise atanmamış.");

            var result = await _adminService.RemoveWorkerFromServiceAsync(serviceEmployee.Id);

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

    [HttpGet("/Admin/Location/GetWorkersByLocation/{locationId}")]
    [AjaxOnly]
    public async Task<IActionResult> GetWorkersByLocation(Guid LocationId)
    {
        var workers = await _adminService.GetAllWorkersAsync(LocationId);
        ViewBag.LocationId = LocationId;
        return PartialView("~/Views/Location/Sections/Worker/PartialView.cshtml", workers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Location/Worker/Add")]
    public async Task<IActionResult> AddWorkerToLocation(WorkerAddDto dto)
    {
        var result = await _adminService.AddWorkerToLocationAsync(dto);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Location/Worker/Edit")]
    public async Task<IActionResult> EditWorkerToLocation(Guid Id, WorkerAddDto dto)
    {
        var result = await _adminService.UpdateWorkerAsync(Id, dto);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Location/Worker/Delete")]
    public async Task<IActionResult> DeleteWorker(Guid id)
    {
        var result = await _adminService.DeleteWorkerAsync(id);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }

    // WORKER HOURS OPERATIONS

    [HttpGet]
    [AjaxOnly]
    [Route("/Admin/Location/GetWorkingHoursByLocation/{locationId}")]
    public async Task<IActionResult> GetWorkingHoursByLocation(Guid locationId)
    {
        var workers = await _adminService.GetAllWorkersAsync(locationId);
        ViewBag.LocationId = locationId;
        return PartialView("~/Views/Location/Sections/WorkerHour/WorkerHourMainPartial.cshtml", workers);
    }

    [HttpGet]
    [AjaxOnly]
    [Route("/Admin/Location/GetWorkerWorkingHours/{workerId}/{year}/{month}")]
    public async Task<IActionResult> GetWorkerWorkingHours(Guid workerId, int year, int month)
    {
        var workingHours = await _adminService.GetWorkerWorkingHoursAsync(workerId, year, month);

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
    [Route("/Admin/Location/WorkerHour/Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkerHour(WorkerWorkingHourDto dto)
    {
        var result = await _adminService.AddWorkerWorkingHourAsync(dto);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }



}