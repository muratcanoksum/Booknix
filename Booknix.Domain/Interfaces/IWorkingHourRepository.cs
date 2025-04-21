using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IWorkingHourRepository
    {
        Task<WorkingHour?> GetByIdAsync(Guid id);
        Task<IEnumerable<WorkingHour>> GetByLocationIdAsync(Guid locationId);
        Task<WorkingHour?> GetByLocationAndDayAsync(Guid locationId, DayOfWeek day);
        
        Task AddAsync(WorkingHour workingHour);
        Task UpdateAsync(WorkingHour workingHour);
        Task DeleteAsync(Guid id);
    }
}
