using Booknix.Application.DTOs;
using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;

namespace Booknix.Application.Interfaces
{
    public interface IWorkerService
    {
        Task<Worker?> GetWorkerByIdAsync(Guid id);
        Task<Worker?> GetWorkerByUserIdAsync(Guid userId);
        Task<List<AppointmentDto>> GetWorkerAppointmentsAsync(Guid workerId);
        Task<bool> UpdateAppointmentStatusAsync(Guid workerId, Guid appointmentId, AppointmentStatus newStatus);
    }
} 