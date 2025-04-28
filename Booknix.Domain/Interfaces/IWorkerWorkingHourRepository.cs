using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IWorkerWorkingHourRepository
    {
        Task<List<WorkerWorkingHour>> GetWorkerWorkingHoursAsync(Guid workerId, int year, int month);
    }
}
