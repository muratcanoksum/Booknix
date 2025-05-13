using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Application.Results;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booknix.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public ReviewService(IReviewRepository reviewRepository, IAppointmentRepository appointmentRepository)
        {
            _reviewRepository = reviewRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<Result<ReviewDto>> GetReviewByIdAsync(Guid id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return Result<ReviewDto>.Failure("Değerlendirme bulunamadı.");
            }

            return Result<ReviewDto>.Success(MapToDto(review));
        }


        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(Guid userId)
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(userId);
            return reviews.Select(MapToDto);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByServiceIdAsync(Guid serviceId)
        {
            var reviews = await _reviewRepository.GetByServiceIdAsync(serviceId);
            return reviews.Select(MapToDto);
        }

        public async Task<double> GetAverageRatingByServiceIdAsync(Guid serviceId)
        {
            return await _reviewRepository.GetAverageRatingByServiceIdAsync(serviceId);
        }

        public async Task<Result<ReviewDto>> CreateReviewAsync(CreateReviewDto reviewDto)
        {
            var existingReview = await _reviewRepository.GetByUserAndAppointmentIdAsync(reviewDto.UserId, reviewDto.AppointmentId);
            if (existingReview != null)
            {
                return Result<ReviewDto>.Failure("Bu randevuyu daha önce değerlendirdiniz.");
            }

            var appointment = await _appointmentRepository.GetByIdAsync(reviewDto.AppointmentId);
            if (appointment == null || appointment.Status != Domain.Entities.Enums.AppointmentStatus.Completed)
            {
                return Result<ReviewDto>.Failure("Sadece tamamlanmış randevular değerlendirilebilir.");
            }

            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
            {
                return Result<ReviewDto>.Failure("Değerlendirme puanı 1-5 arasında olmalıdır.");
            }

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = reviewDto.UserId,
                ServiceId = appointment.ServiceId, // 🧠 Burayı Appointment'tan alıyoruz!
                AppointmentId = appointment.Id,    // 🧠 AppointmentId setleniyor!
                Rating = reviewDto.Rating,
                Comment = reviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            return Result<ReviewDto>.Success(MapToDto(review));
        }

        public async Task<Result<ReviewDto>> UpdateReviewAsync(Guid userId, UpdateReviewDto reviewDto)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewDto.Id);
            if (review == null)
            {
                return Result<ReviewDto>.Failure("Değerlendirme bulunamadı.");
            }

            if (review.UserId != userId)
            {
                return Result<ReviewDto>.Failure("Bu değerlendirmeyi güncelleme yetkiniz yok.");
            }

            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
            {
                return Result<ReviewDto>.Failure("Değerlendirme puanı 1-5 arasında olmalıdır.");
            }

            review.Rating = reviewDto.Rating;
            review.Comment = reviewDto.Comment;

            await _reviewRepository.UpdateAsync(review);
            return Result<ReviewDto>.Success(MapToDto(review));
        }

        public async Task<Result> DeleteReviewAsync(Guid id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null)
            {
                return Result.Failure("Değerlendirme bulunamadı.");
            }

            await _reviewRepository.DeleteAsync(id);
            return Result.Success("Değerlendirme başarıyla silindi.");
        }

        public async Task<bool> HasUserReviewedAppointmentAsync(Guid userId, Guid serviceId)
        {
            var review = await _reviewRepository.GetByUserAndServiceIdAsync(userId, serviceId);
            return review != null;
        }

        private ReviewDto MapToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                ServiceId = review.ServiceId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UserName = review.User?.FullName ?? string.Empty,
                ServiceName = review.Service?.Name ?? string.Empty
            };
        }

        public async Task<Result<ReviewDto>> GetUserReviewAsync(Guid userId, Guid serviceId)
        {
            var review = await _reviewRepository.GetByUserAndServiceIdAsync(userId, serviceId);
            if (review == null)
                return Result<ReviewDto>.Failure("Review bulunamadı.");

            return Result<ReviewDto>.Success(MapToDto(review));
        }

        public async Task<Result<ReviewDto>> GetUserReviewByAppointmentIdAsync(Guid userId, Guid appointmentId)
        {
            var review = await _reviewRepository.GetByUserAndAppointmentIdAsync(userId, appointmentId);
            if (review == null)
                return Result<ReviewDto>.Failure("Yorum bulunamadı.");

            return Result<ReviewDto>.Success(MapToDto(review));
        }

        public async Task<ReviewDto> GetReviewByAppointmentIdAsync(Guid appointmentId)
        {
            var review = await _reviewRepository.GetByAppointmentIdAsync(appointmentId);
            if (review == null)
                return null;

            return MapToDto(review);
        }
    }
}
