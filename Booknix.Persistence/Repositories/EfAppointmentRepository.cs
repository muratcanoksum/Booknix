using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        return _context.Appointments
            .Include(a => a.AppointmentSlot)
            .Where(a => 
                a.AppointmentSlot.AssignerWorkerId == workerId && 
                a.AppointmentSlot.StartTime >= start && 
                a.AppointmentSlot.StartTime <= end)
            .ToList();
    }

    public async Task<List<Appointment>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Appointments
            .Where(a => a.UserId == userId)
            .Include(a => a.AppointmentSlot)
                .ThenInclude(s => s.AssignerWorker)
            .Include(a => a.Service)
                .ThenInclude(s => s.Location)
            .OrderByDescending(a => a.AppointmentSlot.StartTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Appointment?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Appointments
            .Include(a => a.AppointmentSlot)
                .ThenInclude(s => s.AssignerWorker)
            .Include(a => a.Service)
                .ThenInclude(s => s.Location)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }
}
