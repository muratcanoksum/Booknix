using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IAppointmentSlotRepository
    {
        Task<List<AppointmentSlot>> GetByAssignerWorkerIdAsync(Guid workerId);
        Task AddAsync(AppointmentSlot appointmentSlot);
        Task UpdateAsync(AppointmentSlot appointmentSlot);
        Task DeleteAsync(Guid id);
    }
}
