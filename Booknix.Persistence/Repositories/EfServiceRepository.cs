using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfServiceRepository : IServiceRepository
    {
        private readonly BooknixDbContext _context;

        public EfServiceRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<Service?> GetByIdAsync(Guid id)
        {
            return await _context.Services
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Service?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Services
                .Include(s => s.Location)
                .Include(s => s.ServiceEmployees)
                    .ThenInclude(se => se.Worker)
                        .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Service>> GetByLocationIdAsync(Guid locationId)
        {
            return await _context.Services
                .Where(s => s.LocationId == locationId)
                .Include(s => s.ServiceEmployees)
                    .ThenInclude(se => se.Worker)
                        .ThenInclude(w => w.User)
                .ToListAsync();
        }


        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _context.Services
                .Include(s => s.Location)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> SearchByNameAsync(string keyword)
        {
            return await _context.Services
                .Where(s => s.Name.ToLower().Contains(keyword.ToLower()))
                .ToListAsync();
        }

        public async Task AddAsync(Service service)
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }

        public List<Service> Search(string query)
        {
            return _context.Services
                .Include(x => x.Location)
                .Where(x => x.Name.ToLower().Contains(query.ToLower()))
                .ToList();
        }

        public Service? GetByIdWithLocationAndEmployees(Guid id)
        {
            return _context.Services
                .Include(s => s.Location)
                .Include(s => s.ServiceEmployees)
                    .ThenInclude(se => se.Worker)
                        .ThenInclude(w => w!.User)
                .FirstOrDefault(s => s.Id == id);
        }

    }
}
