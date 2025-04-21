using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IMediaFileRepository
    {
        Task<MediaFile?> GetByIdAsync(Guid id);
        Task<IEnumerable<MediaFile>> GetAllAsync();
        Task<IEnumerable<MediaFile>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<MediaFile>> GetByLocationIdAsync(Guid locationId);
        Task<IEnumerable<MediaFile>> GetByServiceIdAsync(Guid serviceId);
        Task<IEnumerable<MediaFile>> GetBySectorIdAsync(Guid sectorId);

        Task AddAsync(MediaFile mediaFile);
        Task UpdateAsync(MediaFile mediaFile);
        Task DeleteAsync(Guid id);
    }
}
