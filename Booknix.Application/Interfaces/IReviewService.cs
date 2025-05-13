using Booknix.Application.DTOs;
using Booknix.Application.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Booknix.Application.Interfaces
{
    public interface IReviewService
    {
        Task<Result<ReviewDto>> GetReviewByIdAsync(Guid id);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(Guid userId);
        Task<IEnumerable<ReviewDto>> GetReviewsByServiceIdAsync(Guid serviceId);
        Task<double> GetAverageRatingByServiceIdAsync(Guid serviceId);
        Task<Result<ReviewDto>> CreateReviewAsync(CreateReviewDto reviewDto);
        Task<Result<ReviewDto>> UpdateReviewAsync(Guid userId, UpdateReviewDto reviewDto); // 🔥 Burayı böyle yap
        Task<Result> DeleteReviewAsync(Guid id);
        Task<bool> HasUserReviewedAppointmentAsync(Guid userId, Guid serviceId);
        Task<Result<ReviewDto>> GetUserReviewAsync(Guid userId, Guid serviceId);
        Task<Result<ReviewDto>> GetUserReviewByAppointmentIdAsync(Guid userId, Guid appointmentId);
        Task<ReviewDto> GetReviewByAppointmentIdAsync(Guid appointmentId);
    }
} 