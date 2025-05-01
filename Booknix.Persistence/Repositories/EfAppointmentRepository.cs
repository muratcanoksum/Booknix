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
}
