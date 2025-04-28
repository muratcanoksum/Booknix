using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfWorkerWorkingHourRepository : IWorkerWorkingHourRepository
    {
        private readonly BooknixDbContext _context;

        public EfWorkerWorkingHourRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkerWorkingHour>> GetWorkerWorkingHoursAsync(Guid workerId, int year, int month)
        {
            return await _context.WorkerWorkingHours
                .Where(x => x.WorkerId == workerId && x.Date.Year == year && x.Date.Month == month)
                .ToListAsync();
        }
    }
}
