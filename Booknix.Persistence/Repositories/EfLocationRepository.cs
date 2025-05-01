using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfLocationRepository : ILocationRepository
    {
        private readonly BooknixDbContext _context;

        public EfLocationRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<Location?> GetByIdAsync(Guid id)
        {
            return await _context.Locations
                .Include(l => l.Sector)
                .Include(l => l.Workers)
                .Include(l => l.Services)
                .Include(l => l.WorkingHours)
                .Include(l => l.MediaFiles)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Location?> GetByNameAsync(string name)
        {
            return await _context.Locations
                .FirstOrDefaultAsync(l => l.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await _context.Locations
                .Include(l => l.Sector)
                .ToListAsync();
        }

        public async Task<IEnumerable<Location>> GetBySectorIdAsync(Guid sectorId)
        {
            return await _context.Locations
                .Where(l => l.SectorId == sectorId)
                .Include(l => l.Sector)
                .ToListAsync();
        }

        public async Task AddAsync(Location location)
        {
            await _context.Locations.AddAsync(location);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Location location)
        {
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Locations.FindAsync(id);
            if (entity != null)
            {
                _context.Locations.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public List<Location> Search(string query)
        {
            return _context.Locations
                .Include(x => x.Sector)
                .Where(x => x.Name.ToLower().Contains(query.ToLower()))
                .ToList();
        }


    }
}
