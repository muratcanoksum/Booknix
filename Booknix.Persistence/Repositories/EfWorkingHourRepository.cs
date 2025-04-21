using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfWorkingHourRepository : IWorkingHourRepository
    {
        private readonly BooknixDbContext _context;

        public EfWorkingHourRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<WorkingHour?> GetByIdAsync(Guid id)
        {
            return await _context.WorkingHours
                .Include(w => w.Location)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<WorkingHour>> GetByLocationIdAsync(Guid locationId)
        {
            return await _context.WorkingHours
                .Where(w => w.LocationId == locationId)
                .OrderBy(w => w.DayOfWeek)
                .ToListAsync();
        }

        public async Task<WorkingHour?> GetByLocationAndDayAsync(Guid locationId, DayOfWeek day)
        {
            return await _context.WorkingHours
                .FirstOrDefaultAsync(w => w.LocationId == locationId && w.DayOfWeek == day);
        }

        public async Task AddAsync(WorkingHour workingHour)
        {
            await _context.WorkingHours.AddAsync(workingHour);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WorkingHour workingHour)
        {
            _context.WorkingHours.Update(workingHour);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.WorkingHours.FindAsync(id);
            if (entity != null)
            {
                _context.WorkingHours.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
