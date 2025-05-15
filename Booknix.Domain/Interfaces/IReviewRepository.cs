using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Review>> GetByServiceIdAsync(Guid serviceId);
        Task<double> GetAverageRatingByServiceIdAsync(Guid serviceId);
        Task<Review?> GetByUserAndServiceIdAsync(Guid userId, Guid serviceId);
        Task<Review?> GetByUserAndAppointmentIdAsync(Guid userId, Guid appointmentId);
        Task<List<Review>> GetByAppointmentIdsAsync(List<Guid> appointmentIds);
        Task<Review?> GetByAppointmentIdAsync(Guid appointmentId);
        Task<(int Count, double AverageRating)> GetWorkerReviewStatsAsync(Guid workerId);

        Task<List<Review>> GetReviewsByLocationAsync(Guid locationId);


        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(Guid id);
    }
}
