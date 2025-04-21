using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface ILocationRepository
    {
        Task<Location?> GetByIdAsync(Guid id);
        Task<Location?> GetByNameAsync(string name);
        Task<IEnumerable<Location>> GetAllAsync();
        Task<IEnumerable<Location>> GetBySectorIdAsync(Guid sectorId);
        Task AddAsync(Location location);
        Task UpdateAsync(Location location);
        Task DeleteAsync(Guid id);
    }
}
