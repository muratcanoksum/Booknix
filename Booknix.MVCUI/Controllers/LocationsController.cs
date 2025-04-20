using Booknix.Application.Helpers;
using Microsoft.AspNetCore.Mvc;

using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.MVCUI.Controllers;

public class LocationsController : Controller
{
    private readonly BooknixDbContext _context;

    public LocationsController(BooknixDbContext context)
    {
        _context = context;
    }

    // /lokasyon/{id}
    [Route("lokasyon/{name}")]
    public async Task<IActionResult> Details(string name)
    {
        var allLocations = await _context.Locations
            .Include(l => l.Services).ThenInclude(s => s.MediaFiles)
            .Include(l => l.MediaFiles)
            .ToListAsync();

        var match = allLocations.FirstOrDefault(l =>
            StringHelper.ToUrlFriendly(l.Name) == name.ToLower());

        if (match == null)
            return NotFound();

        return View(match);
    }

}
