using Microsoft.AspNetCore.Mvc;
using Booknix.Infrastructure.Filters;

namespace Booknix.MVCUI.Controllers;

[Admin]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}