using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;
using Booknix.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booknix.MVCUI.Controllers;

public class AppointmentsController : Controller
{
    private readonly BooknixDbContext _context;

    public AppointmentsController(BooknixDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Route("randevu/{employeeId:guid}/{serviceId:guid}")]
    public async Task<IActionResult> Book(Guid employeeId, Guid serviceId)
    {
        var slots = await _context.AppointmentSlots
            .Include(s => s.Location)
            .Include(s => s.Service)
            .Where(s => s.AssignedEmployeeId == employeeId
                        && s.ServiceId == serviceId
                        && s.Status == SlotStatus.Available
                        && s.StartTime > DateTime.UtcNow)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        ViewBag.EmployeeId = employeeId;
        ViewBag.ServiceId = serviceId;

        return View(slots);
    }

    [HttpPost]
    public async Task<IActionResult> Confirm(Guid EmployeeId, Guid ServiceId, Guid SelectedSlotId)
    {
        // NOT: Gerçek sistemde kullanıcı oturumdan alınmalı
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Auth");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userId),
            ServiceId = ServiceId,
            AppointmentSlotId = SelectedSlotId,
            Status = AppointmentStatus.Pending
        };

        var slot = await _context.AppointmentSlots.FindAsync(SelectedSlotId);
        if (slot != null)
            slot.Status = SlotStatus.Booked;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Success");
    }

    public IActionResult Success()
    {
        return View();
    }
}
