using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAsync();
    List<Appointment> GetByWorkerBetweenDates(Guid workerId, DateTime start, DateTime end);
    Task<List<Appointment>> GetByUserIdAsync(Guid userId);
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<Appointment?> GetByIdWithDetailsAsync(Guid id);
    Task UpdateAsync(Appointment appointment);
}
