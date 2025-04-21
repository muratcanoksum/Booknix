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

    public async Task<IActionResult> Details(string name)
    {
        return View();
    }
}