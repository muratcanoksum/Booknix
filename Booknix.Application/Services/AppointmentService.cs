using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class AppointmentService(IAppointmentRepository appointmentRepository, IReviewRepository reviewRepository, INotificationService notificationService) : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;
        private readonly IReviewRepository _reviewRepository = reviewRepository;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<List<AppointmentDto>> GetUserAppointmentsAsync(Guid userId)
        {
            var appointments = await _appointmentRepository.GetByUserIdAsync(userId);

            var result = appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentSlot?.StartTime.Date ?? DateTime.MinValue,
                StartTime = a.AppointmentSlot?.StartTime.ToString("HH:mm") ?? string.Empty,
                EndTime = a.AppointmentSlot?.EndTime.ToString("HH:mm") ?? string.Empty,
                LocationName = a.Service?.Location?.Name ?? string.Empty,
                WorkerName = a.AppointmentSlot?.AssignerWorker?.User?.FullName ?? "Belirtilmemiş",
                Status = a.Status,
                ServiceName = a.Service?.Name ?? string.Empty,
                ServiceId = a.ServiceId,
                ReviewRating = a.Review?.Rating
            }).ToList();

            return result;
        }

        public async Task<AppointmentDetailDto?> GetAppointmentDetailAsync(Guid userId, Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);

            if (appointment == null || appointment.UserId != userId)
            {
                return null;
            }

            // Check if the appointment can be cancelled
            bool canCancel = false;
            if (appointment.Status != AppointmentStatus.Completed &&
                appointment.Status != AppointmentStatus.Cancelled &&
                appointment.Status != AppointmentStatus.NoShow)
            {
                // Check if appointment date is in the future
                var appointmentDate = appointment.AppointmentSlot?.StartTime ?? DateTime.MinValue;
                canCancel = appointmentDate > DateTime.Now;
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
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt,
                CanCancel = canCancel
            };
        }

        public async Task<bool> CancelAppointmentAsync(Guid userId, Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);

            if (appointment == null || appointment.UserId != userId)
            {
                return false;
            }

            // Cannot cancel if status is Completed, Cancelled, or NoShow
            if (appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Cancelled ||
                appointment.Status == AppointmentStatus.NoShow)
            {
                return false;
            }

            // Cannot cancel if appointment date has passed
            var appointmentDate = appointment.AppointmentSlot?.StartTime ?? DateTime.MinValue;
            if (appointmentDate <= DateTime.Now)
            {
                return false;
            }

            appointment.Status = AppointmentStatus.Cancelled;


            await _appointmentRepository.UpdateAsync(appointment);

            await _notificationService.AddNotificationAsync(
                appointment.UserId,
                "Randevu İptal Edildi",
                $"'{appointment.Service?.Name}' hizmeti için {appointmentDate:dd.MM.yyyy HH:mm} tarihinde olan randevunuzu başarıyla iptal ettiniz.",
                NotificationType.Success
            );


            return true;
        }

        public async Task<List<AppointmentDto>> GetWorkerAppointmentsAsync(Guid workerId)
        {
            var appointments = await _appointmentRepository.GetByWorkerIdAsync(workerId);

            // Randevuların ID'lerini al
            var appointmentIds = appointments.Select(a => a.Id).ToList();

            // Bu ID'lere ait yorumları getir
            var reviews = await _reviewRepository.GetByAppointmentIdsAsync(appointmentIds);

            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentSlot?.StartTime.Date ?? DateTime.MinValue,
                StartTime = a.AppointmentSlot?.StartTime.ToString("HH:mm") ?? string.Empty,
                EndTime = a.AppointmentSlot?.EndTime.ToString("HH:mm") ?? string.Empty,
                LocationName = a.Service?.Location?.Name ?? string.Empty,
                WorkerName = a.AppointmentSlot?.AssignerWorker?.User?.FullName ?? "Belirtilmemiş",
                UserName = a.User?.FullName ?? "Belirtilmemiş",
                Status = a.Status,
                ServiceName = a.Service?.Name ?? string.Empty,
                ServiceId = a.ServiceId,
                ReviewRating = reviews.FirstOrDefault(r => r.AppointmentId == a.Id)?.Rating
            }).ToList();
        }

        public async Task<List<AppointmentDto>> GetAppointmentsByLocationAsync(Guid locationId)
        {
            var appointments = await _appointmentRepository.GetAppointmentsByLocationAsync(locationId);

            return appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentSlot?.StartTime.Date ?? DateTime.MinValue,
                StartTime = a.AppointmentSlot?.StartTime.ToString(@"hh\:mm") ?? string.Empty,
                EndTime = a.AppointmentSlot?.EndTime.ToString(@"hh\:mm") ?? string.Empty,
                LocationName = a.Service?.Location?.Name ?? string.Empty,
                WorkerName = a.AppointmentSlot?.AssignerWorker?.User?.FullName ?? "Belirtilmemiş",
                UserName = a.User?.FullName ?? "Belirtilmemiş",
                Status = a.Status,
                ServiceName = a.Service?.Name ?? string.Empty,
                ServiceId = a.ServiceId,
                ReviewRating = a.Review?.Rating
            }).ToList();
        }

        public async Task<bool> UpdateAppointmentStatusAsync(Guid appointmentId, AppointmentStatus status)
        {
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId);
            if (appointment == null)
            {
                return false;
            }

            appointment.Status = status;
            await _appointmentRepository.UpdateAsync(appointment);

            // Bildirim gönder
            string statusMessage = status switch
            {
                AppointmentStatus.Approved => "onaylandı",
                AppointmentStatus.Completed => "tamamlandı",
                AppointmentStatus.Cancelled => "iptal edildi",
                AppointmentStatus.NoShow => "gelmedi olarak işaretlendi",
                _ => "güncellendi"
            };

            string serviceName = appointment.Service?.Name ?? "Randevu";
            DateTime appointmentDate = appointment.AppointmentSlot?.StartTime ?? DateTime.MinValue;
            string dateStr = appointmentDate.ToString("dd.MM.yyyy HH:mm");

            var notificationType = status switch
            {
                AppointmentStatus.Approved => NotificationType.Success,
                AppointmentStatus.Completed => NotificationType.Success,
                AppointmentStatus.Cancelled => NotificationType.Info,
                AppointmentStatus.NoShow => NotificationType.Info,
                _ => NotificationType.Info
            };

            await _notificationService.AddNotificationAsync(
                appointment.UserId,
                $"Randevu {statusMessage}",
                $"'{serviceName}' hizmeti için {dateStr} tarihindeki randevunuz lokasyon yöneticisi tarafından {statusMessage}.",
                notificationType
            );

            return true;
        }
    }
}