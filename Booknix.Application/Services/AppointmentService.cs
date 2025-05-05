using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<List<AppointmentDto>> GetUserAppointmentsAsync(Guid userId)
        {
            var appointments = await _appointmentRepository.GetByUserIdAsync(userId);
            
            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentSlot?.StartTime.Date ?? DateTime.MinValue,
                StartTime = a.AppointmentSlot?.StartTime.ToString("HH:mm") ?? string.Empty,
                EndTime = a.AppointmentSlot?.EndTime.ToString("HH:mm") ?? string.Empty,
                LocationName = a.Service?.Location?.Name ?? string.Empty,
                WorkerName = a.AppointmentSlot?.AssignerWorker?.User?.FullName ?? "Belirtilmemiş",
                Status = a.Status.ToString(),
                ServiceName = a.Service?.Name ?? string.Empty
            }).ToList();
        }

        public async Task<AppointmentDetailDto?> GetAppointmentDetailAsync(Guid userId, Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
            
            if (appointment == null || appointment.UserId != userId)
            {
                return null;
            }
            
            return new AppointmentDetailDto
            {
                Id = appointment.Id,
                AppointmentDate = appointment.AppointmentSlot?.StartTime.Date ?? DateTime.MinValue,
                StartTime = appointment.AppointmentSlot?.StartTime.ToString("HH:mm") ?? string.Empty,
                EndTime = appointment.AppointmentSlot?.EndTime.ToString("HH:mm") ?? string.Empty,
                LocationName = appointment.Service?.Location?.Name ?? string.Empty,
                LocationAddress = appointment.Service?.Location?.Address ?? string.Empty,
                LocationPhone = appointment.Service?.Location?.PhoneNumber ?? string.Empty,
                ServiceName = appointment.Service?.Name ?? string.Empty,
                WorkerName = appointment.AppointmentSlot?.AssignerWorker?.User?.FullName ?? "Belirtilmemiş",
                Status = appointment.Status.ToString(),
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt
            };
        }

        public async Task<bool> CancelAppointmentAsync(Guid userId, Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            
            if (appointment == null || appointment.UserId != userId)
            {
                return false;
            }
            
            if (appointment.Status != AppointmentStatus.Pending && 
                appointment.Status != AppointmentStatus.Approved)
            {
                return false;
            }
            
            appointment.Status = AppointmentStatus.Cancelled;
            await _appointmentRepository.UpdateAsync(appointment);
            
            return true;
        }
    }
} 