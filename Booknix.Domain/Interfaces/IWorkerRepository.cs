using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IWorkerRepository
    {
        Task<Worker?> GetByIdAsync(Guid id);
        Task<IEnumerable<Worker>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Worker>> GetByLocationIdAsync(Guid locationId);
        Task<Worker?> GetByUserAndLocationAsync(Guid userId, Guid locationId);
        Task<bool> ExistsAsync(Guid userId, Guid locationId);

        Task AddAsync(Worker userLocation);
        Task UpdateAsync(Worker userLocation);
        Task DeleteAsync(Guid id);
    }
}
