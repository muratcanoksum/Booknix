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
                .Where(x => x.WorkerId == workerId &&
                            x.Date.Year == year &&
                            x.Date.Month == month)
                .ToListAsync();
        }

        public async Task AddAsync(WorkerWorkingHour entity)
        {
            await _context.WorkerWorkingHours.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WorkerWorkingHour entity)
        {
            _context.WorkerWorkingHours.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<WorkerWorkingHour?> GetByWorkerIdAndDateAsync(Guid workerId, DateTime date)
        {
            return await _context.WorkerWorkingHours
                .FirstOrDefaultAsync(x => x.WorkerId == workerId && x.Date.Date == date.Date);
        }
        public async Task AddRangeAsync(IEnumerable<WorkerWorkingHour> entities)
        {
            await _context.WorkerWorkingHours.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<WorkerWorkingHour> entities)
        {
            _context.WorkerWorkingHours.UpdateRange(entities);
            await _context.SaveChangesAsync();
        }

    }
}
