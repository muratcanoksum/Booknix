using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfServiceEmployeeRepository : IServiceEmployeeRepository
    {
        private readonly BooknixDbContext _context;

        public EfServiceEmployeeRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceEmployee?> GetByIdAsync(Guid id)
        {
            return await _context.ServiceEmployees
                .Include(se => se.Worker)
                .Include(se => se.Service)
                .FirstOrDefaultAsync(se => se.Id == id);
        }

        public async Task<IEnumerable<ServiceEmployee>> GetByServiceIdAsync(Guid serviceId)
        {
            return await _context.ServiceEmployees
                .Where(se => se.ServiceId == serviceId)
                .Include(se => se.Worker)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceEmployee>> GetByEmployeeIdAsync(Guid employeeId)
        {
            return await _context.ServiceEmployees
                .Where(se => se.WorkerId == employeeId)
                .Include(se => se.Service)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid serviceId, Guid employeeId)
        {
            return await _context.ServiceEmployees
                .AnyAsync(se => se.ServiceId == serviceId && se.WorkerId == employeeId);
        }

        public async Task AddAsync(ServiceEmployee entity)
        {
            await _context.ServiceEmployees.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.ServiceEmployees.FindAsync(id);
            if (entity != null)
            {
                _context.ServiceEmployees.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<ServiceEmployee> entities)
        {
            await _context.ServiceEmployees.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

    }
}
