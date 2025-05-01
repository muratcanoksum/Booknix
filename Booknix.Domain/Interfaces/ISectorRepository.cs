using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface ISectorRepository
    {
        Task<Sector?> GetByIdAsync(Guid id);
        Task<Sector?> GetByNameAsync(string name);
        Task<IEnumerable<Sector>> GetAllAsync();
        Task AddAsync(Sector sector);
        Task UpdateAsync(Sector sector);
        Task DeleteAsync(Guid id);
        Task<List<Sector>> GetAllWithLocationsAndMediaAsync();
    }
}

