using System.Diagnostics;
using Booknix.MVCUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.MVCUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
