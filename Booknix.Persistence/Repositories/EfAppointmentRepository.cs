using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories;

public class EfAppointmentRepository : IAppointmentRepository
{
    private readonly BooknixDbContext _context;

    public EfAppointmentRepository(BooknixDbContext context)
    {
        _context = context;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _context.Appointments.ToListAsync();
    }

    public List<Appointment> GetByWorkerBetweenDates(Guid workerId, DateTime start, DateTime end)
    {
        var startUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        return _context.Appointments
            .Include(a => a.AppointmentSlot)
            .Where(a =>
                a.AppointmentSlot!.AssignerWorkerId == workerId &&
                a.AppointmentSlot.StartTime >= startUtc &&
                a.AppointmentSlot.StartTime <= endUtc)
            .ToList();
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .Include(a => a.AppointmentSlot)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
    }


    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var appointment = await GetByIdAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
    }


}
