using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfUserLocationRepository : IUserLocationRepository
    {
        private readonly BooknixDbContext _context;

        public EfUserLocationRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<UserLocation?> GetByIdAsync(Guid id)
        {
            return await _context.UserLocations
                .Include(ul => ul.User)
                .Include(ul => ul.Location)
                .FirstOrDefaultAsync(ul => ul.Id == id);
        }

        public async Task<IEnumerable<UserLocation>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserLocations
                .Where(ul => ul.UserId == userId)
                .Include(ul => ul.Location)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserLocation>> GetByLocationIdAsync(Guid locationId)
        {
            return await _context.UserLocations
                .Where(ul => ul.LocationId == locationId)
                .Include(ul => ul.User)
                .ToListAsync();
        }

        public async Task<UserLocation?> GetByUserAndLocationAsync(Guid userId, Guid locationId)
        {
            return await _context.UserLocations
                .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.LocationId == locationId);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid locationId)
        {
            return await _context.UserLocations
                .AnyAsync(ul => ul.UserId == userId && ul.LocationId == locationId);
        }

        public async Task AddAsync(UserLocation userLocation)
        {
            await _context.UserLocations.AddAsync(userLocation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserLocation userLocation)
        {
            _context.UserLocations.Update(userLocation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.UserLocations.FindAsync(id);
            if (entity != null)
            {
                _context.UserLocations.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
