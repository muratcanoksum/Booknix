using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        return await _context.Appointments
            .Include(a => a.AppointmentSlot)
                .ThenInclude(slot => slot.AssignerWorker)
                    .ThenInclude(worker => worker.User)
            .Include(a => a.Service)
                .ThenInclude(service => service.Location)
            .Include(a => a.User)
            .ToListAsync();
    }

    public List<Appointment> GetByWorkerBetweenDates(Guid workerId, DateTime start, DateTime end)
    {
        start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        return _context.Appointments
            .Include(a => a.AppointmentSlot)
            .Where(a =>
                a.AppointmentSlot!.AssignerWorkerId == workerId &&
                a.AppointmentSlot.StartTime >= start &&
                a.AppointmentSlot.StartTime <= end)
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

    public async Task<List<Appointment>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Appointments
            .Where(a => a.UserId == userId)
            .Include(a => a.AppointmentSlot)
                .ThenInclude(s => s.AssignerWorker)
                    .ThenInclude(w => w.User) // eksik olan burasıydı
            .Include(a => a.Service)
                .ThenInclude(s => s.Location)
            .OrderByDescending(a => a.AppointmentSlot.StartTime)
            .ToListAsync();
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

    public async Task<List<Appointment>> GetByWorkerIdAsync(Guid workerId)
    {
        return await _context.Appointments
            .Include(a => a.AppointmentSlot)
                .ThenInclude(s => s.AssignerWorker)
                    .ThenInclude(w => w.User)
            .Include(a => a.Service)
                .ThenInclude(s => s.Location)
            .Include(a => a.User)
            .Where(a => a.AppointmentSlot.AssignerWorkerId == workerId)
            .OrderByDescending(a => a.AppointmentSlot.StartTime)
            .ToListAsync();
    }

    public async Task<List<Review>> GetReviewsByLocationAsync(Guid locationId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Service)
            .Where(r => r.Service.LocationId == locationId)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetAppointmentsByLocationAsync(Guid locationId)
    {
        return await _context.Appointments
            .Include(a => a.AppointmentSlot)
                .ThenInclude(slot => slot.AssignerWorker)
                    .ThenInclude(worker => worker.User)
            .Include(a => a.User)
            .Include(a => a.Service)
                .ThenInclude(s => s.Location)
            .Include(a => a.Review)
            .Where(a => a.Service.LocationId == locationId)
            .OrderBy(a => a.Status == Domain.Entities.Enums.AppointmentStatus.Pending ? 0 : 1)
            .ThenByDescending(a => a.AppointmentSlot.StartTime)
            .ToListAsync();
    }



}
