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
}
