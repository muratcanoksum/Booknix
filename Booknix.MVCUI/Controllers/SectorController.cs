using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booknix.MVCUI.Controllers.Sector
{
    [Route("Admin/Sectors")]
    public class SectorController : Controller
    {
        private readonly ISectorService _sectorService;

        public SectorController(ISectorService sectorService)
        {
            _sectorService = sectorService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var sectors = await _sectorService.GetAllAsync();
            return View(sectors);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(SectorDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _sectorService.AddAsync(dto);
            return RedirectToAction("Index");
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var sector = await _sectorService.GetByIdAsync(id);
            if (sector == null) return NotFound();
            return View(sector);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id, SectorDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _sectorService.UpdateAsync(dto);
            return RedirectToAction("Index");
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _sectorService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
