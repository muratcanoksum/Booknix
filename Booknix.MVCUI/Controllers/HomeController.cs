using System.Diagnostics;
using Booknix.Application.Interfaces;
using Booknix.MVCUI.Models;
using Booknix.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booknix.MVCUI.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {

        }

        public IActionResult Index()
        {
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Role = HttpContext.Session.GetString("Role");

            //var sectors = await _homeService.GetSectorsAsync();
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}