using Booknix.Application.DTOs;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfSectorRepository : ISectorRepository
    {
        private readonly BooknixDbContext _context;

        public EfSectorRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Sector sector)
        {
            await _context.Sectors.AddAsync(sector);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var sector = await _context.Sectors.FindAsync(id);
            if (sector != null)
            {
                _context.Sectors.Remove(sector);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Sector>> GetAllAsync()
        {
            return await _context.Sectors
                .Include(s => s.MediaFiles)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Sector?> GetByIdAsync(Guid id)
        {
            return await _context.Sectors
                .Include(s => s.MediaFiles)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sector?> GetByNameAsync(string name)
        {
            return await _context.Sectors
                .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
        }

        public async Task UpdateAsync(Sector sector)
        {
            _context.Sectors.Update(sector);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Sector>> GetAllWithLocationsAndMediaAsync()
        {
            return await _context.Sectors
        .Include(s => s.Locations)  // Locations'ý dahil et
            .ThenInclude(l => l.Services)  // Services'ý dahil et
        .Include(s => s.Locations)  // Locations'ý tekrar dahil et
            .ThenInclude(l => l.Workers)  // Workers'ý dahil et
        .Include(s => s.MediaFiles)  // MediaFiles'ý dahil et
        .ToListAsync();
        }



    }
}
