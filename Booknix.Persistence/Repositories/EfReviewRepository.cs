using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfReviewRepository : IReviewRepository
    {
        private readonly BooknixDbContext _context;

        public EfReviewRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetByIdAsync(Guid id)
        {
            return await _context.Reviews.FindAsync(id);
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByServiceIdAsync(Guid serviceId)
        {
            return await _context.Reviews
                .Where(r => r.ServiceId == serviceId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingByServiceIdAsync(Guid serviceId)
        {
            return await _context.Reviews
                .Where(r => r.ServiceId == serviceId)
                .Select(r => (double?)r.Rating)
                .AverageAsync() ?? 0.0;
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }
    }
}
