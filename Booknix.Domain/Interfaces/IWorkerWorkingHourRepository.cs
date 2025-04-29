using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IWorkerWorkingHourRepository
    {
        Task<List<WorkerWorkingHour>> GetWorkerWorkingHoursAsync(Guid workerId, int year, int month);
        Task AddAsync(WorkerWorkingHour entity);
        Task UpdateAsync(WorkerWorkingHour entity);
        Task<WorkerWorkingHour?> GetByWorkerIdAndDateAsync(Guid workerId, DateTime date);

    }
}
