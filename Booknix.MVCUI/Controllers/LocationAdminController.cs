using Booknix.Application.Interfaces;
using Booknix.Application.Services;
using Booknix.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

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
    }
}
