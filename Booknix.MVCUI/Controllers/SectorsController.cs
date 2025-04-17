using Booknix.Application.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Booknix.Persistence.Data;

namespace Booknix.MVCUI.Controllers
{
    public class SectorsController : Controller
    {
        private readonly BooknixDbContext _context;

        public SectorsController(BooknixDbContext context)
        {
            _context = context;
        }

        [Route("sektor/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var all = await _context.Sectors.ToListAsync();

            var match = all.FirstOrDefault(s =>
                StringHelper.ToUrlFriendly(s.Name) == slug.ToLower());

            if (match == null)
                return NotFound();

            // locations, media eklenmiş hali
            var sector = await _context.Sectors
                .Include(s => s.Locations)
                .ThenInclude(l => l.MediaFiles)
                .FirstOrDefaultAsync(s => s.Id == match.Id);

            return View(sector);
        }

    }
}