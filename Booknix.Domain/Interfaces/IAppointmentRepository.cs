using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAsync();
    List<Appointment> GetByWorkerBetweenDates(Guid workerId, DateTime start, DateTime end);

    Task<Appointment?> GetByIdAsync(Guid id);
    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(Guid id);
    Task<List<Appointment>> GetByUserIdAsync(Guid userId);
    Task<Appointment?> GetByIdWithDetailsAsync(Guid id);
}
