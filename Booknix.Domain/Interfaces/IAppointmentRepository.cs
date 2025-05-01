using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAsync();
    List<Appointment> GetByWorkerBetweenDates(Guid workerId, DateTime start, DateTime end);

}
