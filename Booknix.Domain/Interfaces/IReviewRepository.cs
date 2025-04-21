using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Review>> GetByServiceIdAsync(Guid serviceId);
        Task<double> GetAverageRatingByServiceIdAsync(Guid serviceId);
        
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(Guid id);
    }
}
