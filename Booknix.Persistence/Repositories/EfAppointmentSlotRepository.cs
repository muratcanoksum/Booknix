using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

public class EfAppointmentSlotRepository : IAppointmentSlotRepository
{
    private readonly BooknixDbContext _context;

    public EfAppointmentSlotRepository(BooknixDbContext context)
    {
        _context = context;
    }

    public async Task<List<AppointmentSlot>> GetByAssignerWorkerIdAsync(Guid workerId)
    {
        return await _context.AppointmentSlots
            .Where(x => x.AssignerWorkerId == workerId)
            .ToListAsync();
    }

    public async Task AddAsync(AppointmentSlot appointmentSlot)
    {
        _context.AppointmentSlots.Add(appointmentSlot);
        await _context.SaveChangesAsync();
    }


    public async Task UpdateAsync(AppointmentSlot appointmentSlot)
    {
        _context.AppointmentSlots.Update(appointmentSlot);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var appointmentSlot = await _context.AppointmentSlots.FindAsync(id);
        if (appointmentSlot != null)
        {
            _context.AppointmentSlots.Remove(appointmentSlot);
            await _context.SaveChangesAsync();
        }
    }

}

