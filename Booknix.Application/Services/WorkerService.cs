using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class WorkerService : IWorkerService
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public WorkerService(
            IWorkerRepository workerRepository,
            IAppointmentRepository appointmentRepository)
        {
            _workerRepository = workerRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<Worker?> GetWorkerByIdAsync(Guid id)
        {
            return await _workerRepository.GetByIdWithDetailsAsync(id);
        }

        public async Task<Worker?> GetWorkerByUserIdAsync(Guid userId)
        {
            return await _workerRepository.GetByUserIdAsync(userId);
        }

        public async Task<List<AppointmentDto>> GetWorkerAppointmentsAsync(Guid workerId)
        {
            // Get appointments where this worker is assigned
            var appointments = await _appointmentRepository.GetByWorkerIdAsync(workerId);
            
            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentSlot?.StartTime.Date ?? DateTime.MinValue,
                StartTime = a.AppointmentSlot?.StartTime.ToString("HH:mm") ?? string.Empty,
                EndTime = a.AppointmentSlot?.EndTime.ToString("HH:mm") ?? string.Empty,
                LocationName = a.Service?.Location?.Name ?? string.Empty,
                WorkerName = a.AppointmentSlot?.AssignerWorker?.User?.FullName ?? "Belirtilmemiş",
                UserName = a.User?.FullName ?? "Belirtilmemiş",
                Status = a.Status.ToString(),
                ServiceName = a.Service?.Name ?? string.Empty
            }).ToList();
        }

        public async Task<bool> UpdateAppointmentStatusAsync(Guid workerId, Guid appointmentId, AppointmentStatus newStatus)
        {
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
            
            // Check if appointment exists and belongs to this worker
            if (appointment == null || appointment.AppointmentSlot?.AssignerWorkerId != workerId)
            {
                return false;
            }
            
            // Check for valid status transitions
            bool isValidTransition = (appointment.Status, newStatus) switch
            {
                // From Pending
                (AppointmentStatus.Pending, AppointmentStatus.Approved) => true,
                (AppointmentStatus.Pending, AppointmentStatus.Cancelled) => true,
                
                // From Approved
                (AppointmentStatus.Approved, AppointmentStatus.Completed) => true,
                (AppointmentStatus.Approved, AppointmentStatus.NoShow) => true,
                (AppointmentStatus.Approved, AppointmentStatus.Cancelled) => true,
                
                // Invalid transitions
                _ => false
            };
            
            if (!isValidTransition)
            {
                return false;
            }
            
            // Update appointment status
            appointment.Status = newStatus;
            await _appointmentRepository.UpdateAsync(appointment);
            
            return true;
        }
    }
} 