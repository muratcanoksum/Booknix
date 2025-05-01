using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IWorkerRepository
    {
        Task<Worker?> GetByIdAsync(Guid id);
        Task<Worker?> GetByIdWithDetailsAsync(Guid id);
        Task<Worker?> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Worker>> GetByLocationIdAsync(Guid locationId);
        Task<Worker?> GetByUserAndLocationAsync(Guid userId, Guid locationId);
        Task<bool> ExistsAsync(Guid userId, Guid locationId);

        Task AddAsync(Worker worker);
        Task UpdateAsync(Worker worker);
        Task DeleteAsync(Guid id);
    }
}
