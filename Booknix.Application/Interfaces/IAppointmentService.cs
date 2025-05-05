using Booknix.Application.DTOs;
using Booknix.Domain.Entities;

namespace Booknix.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<AppointmentDto>> GetUserAppointmentsAsync(Guid userId);
        Task<AppointmentDetailDto?> GetAppointmentDetailAsync(Guid userId, Guid appointmentId);
        Task<bool> CancelAppointmentAsync(Guid userId, Guid appointmentId);
        Task<List<AppointmentDto>> GetWorkerAppointmentsAsync(Guid workerId);
    }
} 