using Booknix.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.MVCUI.Controllers;

public class HomeController : Controller
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    public async Task<IActionResult> Index()
    {
        var dto = await _homeService.GetHomePageDataAsync();
        return View(dto);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("/api/search")]
    public IActionResult Search(string q)
    {
        var results = _homeService.SearchLocationsAndServices(q);
        return Ok(results);
    }


}
