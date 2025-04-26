using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IServiceEmployeeRepository
    {
        Task<ServiceEmployee?> GetByIdAsync(Guid id);
        Task<IEnumerable<ServiceEmployee>> GetByServiceIdAsync(Guid serviceId);
        Task<IEnumerable<ServiceEmployee>> GetByEmployeeIdAsync(Guid employeeId);
        Task<bool> ExistsAsync(Guid serviceId, Guid employeeId);
        Task<ServiceEmployee?> GetByServiceAndWorkerIdAsync(Guid serviceId, Guid workerId);
        Task AddRangeAsync(IEnumerable<ServiceEmployee> entities);

        Task AddAsync(ServiceEmployee entity);
        Task DeleteAsync(Guid id);
    }
}
