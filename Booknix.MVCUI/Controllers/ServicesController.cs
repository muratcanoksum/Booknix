using Booknix.Application.Helpers;
using Booknix.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booknix.MVCUI.Controllers;

public class ServicesController : Controller
{
    private readonly BooknixDbContext _context;

    public ServicesController(BooknixDbContext context)
    {
        _context = context;
    }

    [Route("hizmet/{name}")]
    public async Task<IActionResult> Details(string name)
    {
        var all = await _context.Services
            .Include(s => s.Location)
            .Include(s => s.MediaFiles)
            .Include(s => s.Appointments)
            .Include(s => s.ServiceEmployees)
            .ThenInclude(se => se.Employee)
            .ThenInclude(e => e.MediaFiles) // 🟢 Buraya dikkat!
            .ToListAsync();
        var service = all.FirstOrDefault(s =>
            StringHelper.ToUrlFriendly(s.Name) == name.ToLower());

        if (service == null)
            return NotFound();

        return View(service);
    }
}