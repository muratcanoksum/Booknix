using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfWorkerRepository : IWorkerRepository
    {
        private readonly BooknixDbContext _context;

        public EfWorkerRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<Worker?> GetByIdAsync(Guid id)
        {
            return await _context.Workers
                .Include(w => w.User)
                .Include(w => w.Location)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<Worker>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Workers
                .Where(w => w.UserId == userId)
                .Include(w => w.Location)
                .ToListAsync();
        }

        public async Task<IEnumerable<Worker>> GetByLocationIdAsync(Guid locationId)
        {
            return await _context.Workers
                .Where(w => w.LocationId == locationId)
                .Include(w => w.User)
                .ToListAsync();
        }

        public async Task<Worker?> GetByUserAndLocationAsync(Guid userId, Guid locationId)
        {
            return await _context.Workers
                .FirstOrDefaultAsync(w => w.UserId == userId && w.LocationId == locationId);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid locationId)
        {
            return await _context.Workers
                .AnyAsync(w => w.UserId == userId && w.LocationId == locationId);
        }

        public async Task AddAsync(Worker worker)
        {
            await _context.Workers.AddAsync(worker);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Worker worker)
        {
            _context.Workers.Update(worker);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Workers.FindAsync(id);
            if (entity != null)
            {
                _context.Workers.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
