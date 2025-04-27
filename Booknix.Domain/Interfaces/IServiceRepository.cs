using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdAsync(Guid id);
        Task<Service?> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<Service>> GetByLocationIdAsync(Guid locationId);
        Task<IEnumerable<Service>> GetAllAsync();
        Task<IEnumerable<Service>> SearchByNameAsync(string keyword);

        Task AddAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(Guid id);
    }
}
