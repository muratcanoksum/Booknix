using Booknix.Application.Interfaces;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;
using Booknix.Application.DTOs;

namespace Booknix.MVCUI.Controllers;

public class PublicController(IPublicService publicService) : Controller
{
    private readonly IPublicService _publicService = publicService;

    public async Task<IActionResult> Index()
    {
        var dto = await _publicService.GetHomePageDataAsync();
        return View(dto);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("/api/search")]
    public IActionResult Search(string q)
    {
        var results = _publicService.SearchLocationsAndServices(q);
        return Ok(results);
    }

    [HttpGet("/sektor/{slug}")]
    public IActionResult LocationsBySector(string slug)
    {
        var model = _publicService.GetLocationsBySectorSlug(slug);
        if (model == null) return NotFound();

        ViewData["Title"] = model.SectorName;
        return View("SectorLocations", model);
    }

    [HttpGet("/lokasyon/{slug}")]
    public IActionResult LocationDetails(string slug)
    {
        var model = _publicService.GetLocationDetails(slug);
        if (model == null) return NotFound();

        ViewData["Title"] = model.Name;
        return View("LocationDetails", model);
    }

    [Auth]
    [HttpGet("/lokasyon/{slug}/{id}")]
    public IActionResult ServiceDetails(string slug, Guid id)
    {
        var model = _publicService.GetServiceDetails(slug, id);
        if (model == null) return NotFound();

        ViewData["Title"] = model.ServiceName;
        return View("ServiceDetails", model);
    }

    [Auth]
    [HttpGet("/randevu/{workerId}")]
    public async Task<IActionResult> AppointmentSlots(Guid workerId, Guid sid)
    {
        var now = DateTime.Now;
        var startDate = now.Date;
        var endDate = startDate.AddDays(7);

        var dto = await _publicService.GetAppointmentSlotPageData(workerId, sid, startDate, endDate, now.TimeOfDay);

        ViewBag.WorkerId = workerId;
        ViewBag.ServiceId = sid;

        return View("AppointmentSlots", dto);
    }


    [Auth]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmAppointment(CreateAppointmentDto dto)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path });

        //var result = await _publicService.CreateAppointmentAsync(userId, dto);

        var result = new RequestResult
        {
            Success = true,
            Message = "Randevu ba�ar�yla olu�turuldu."
        };
        {

        }

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            //return RedirectToAction("ServiceDetails", new { slug = dto.Slug, id = dto.ServiceId });
            BadRequest(result.Message);
        }

        TempData["Success"] = "Randevunuz ba�ar�yla olu�turuldu.";
        //return RedirectToAction("MyAppointments", "Account");
        return Ok(result.Message);
    }





}
