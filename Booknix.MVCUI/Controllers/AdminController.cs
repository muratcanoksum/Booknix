using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Booknix.Infrastructure.Filters;
using Booknix.Application.Interfaces;
using Booknix.Application.DTOs;
using Booknix.Domain.Entities;

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
            return NotFound("Sektör bulunamadý.");
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
            return BadRequest("Tüm alanlarýn eksiksiz doldurulduðundan emin olun.");
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
            return NotFound("Lokasyon bulunamadý.");
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


    // LOCATION DETAÝLS OPERATIONS



    [HttpGet("Admin/Location/Details/{id}")]
    public async Task<IActionResult> LocationDetails(Guid id)
    {
        var location = await _adminService.GetLocationByIdAsync(id);
        if (location == null)
            return NotFound("Lokasyon bulunamadý.");

        // Ýlgili diðer veriler varsa burada alýnabilir (örneðin çalýþanlar)
        // var employees = await _employeeService.GetByLocationIdAsync(id);

        return View("Location/Details", location);
    }


    // SERVICE OPERATIONS

    [HttpGet("/Admin/GetServicesByLocation/{locationId}")]
    public async Task<IActionResult> GetServicesByLocation(Guid locationId)
    {
        var services = await _adminService.GetServicesByLocationAsync(locationId);
        ViewBag.LocationId = locationId;
        return PartialView("Location/LocationModules/Service/ServiceListPartial", services);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddServiceToLocation(ServiceCreateDto dto)
    {
        if (!await _adminService.LocationExistsAsync(dto.LocationId))
        {
            return BadRequest("Lokasyon bulunamadý. Lütfen geçerli bir lokasyon seçiniz.");
        }

        var result = await _adminService.AddServiceToLocationAsync(dto);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }


    // WORKER OPERATIONS

    [HttpGet("/Admin/GetWorkersByLocation/{locationId}")]
    public async Task<IActionResult> GetWorkersByLocation(Guid LocationId)
    {
        var workers = await _adminService.GetAllWorkersAsync(LocationId);
        ViewBag.LocationId = LocationId;
        return PartialView("Location/LocationModules/Worker/PartialView", workers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Worker/Create")]
    public async Task<IActionResult> AddWorkerToLocation(WorkerAddDto dto)
    {
        var result = await _adminService.AddWorkerToLocationAsync(dto);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok();

    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Admin/Worker/Delete")]
    public async Task<IActionResult> DeleteWorker(Guid id)
    {
        var result = await _adminService.DeleteWorkerAsync(id);
        if (!result.Success)
            return BadRequest(result.Message);

        return Ok();
    }






}