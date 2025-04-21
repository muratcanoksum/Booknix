using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfMediaFileRepository : IMediaFileRepository
    {
        private readonly BooknixDbContext _context;

        public EfMediaFileRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<MediaFile?> GetByIdAsync(Guid id)
        {
            return await _context.MediaFiles.FindAsync(id);
        }

        public async Task<IEnumerable<MediaFile>> GetAllAsync()
        {
            return await _context.MediaFiles.ToListAsync();
        }

        public async Task<IEnumerable<MediaFile>> GetByUserIdAsync(Guid userId)
        {
            return await _context.MediaFiles
                .Where(m => m.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MediaFile>> GetByLocationIdAsync(Guid locationId)
        {
            return await _context.MediaFiles
                .Where(m => m.LocationId == locationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MediaFile>> GetByServiceIdAsync(Guid serviceId)
        {
            return await _context.MediaFiles
                .Where(m => m.ServiceId == serviceId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MediaFile>> GetBySectorIdAsync(Guid sectorId)
        {
            return await _context.MediaFiles
                .Where(m => m.SectorId == sectorId)
                .ToListAsync();
        }

        public async Task AddAsync(MediaFile mediaFile)
        {
            await _context.MediaFiles.AddAsync(mediaFile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MediaFile mediaFile)
        {
            _context.MediaFiles.Update(mediaFile);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var mediaFile = await GetByIdAsync(id);
            if (mediaFile != null)
            {
                _context.MediaFiles.Remove(mediaFile);
                await _context.SaveChangesAsync();
            }
        }
    }
}
