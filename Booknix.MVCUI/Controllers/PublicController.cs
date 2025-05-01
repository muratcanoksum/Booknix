using Booknix.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    [HttpGet("/lokasyon/{slug}/{id}")]
    public IActionResult ServiceDetails(string slug, Guid id)
    {
        var model = _publicService.GetServiceDetails(slug, id);
        if (model == null) return NotFound();

        ViewData["Title"] = model.ServiceName;
        return View("ServiceDetails", model);
    }

    [HttpGet("/randevu/{workerId}")]
    public async Task<IActionResult> AppointmentSlots(Guid workerId, Guid sid)
    {
        var now = DateTime.Now;
        var startDate = now.Date;
        var endDate = startDate.AddDays(7);

        var dto = await _publicService.GetAppointmentSlotPageData(workerId, sid, startDate, endDate, now.TimeOfDay);
        return View("AppointmentSlots", dto);
    }






}
