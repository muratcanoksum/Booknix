using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IAppointmentSlotRepository
    {
        Task<List<AppointmentSlot>> GetByAssignerWorkerIdAsync(Guid workerId);
    }
}
