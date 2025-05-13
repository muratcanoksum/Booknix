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
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Reviews
                .Include(r => r.Service)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByServiceIdAsync(Guid serviceId)
        {
            return await _context.Reviews
                .Include(r => r.User)
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

        public async Task<Review?> GetByUserAndServiceIdAsync(Guid userId, Guid serviceId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ServiceId == serviceId);
        }

        public async Task<Review?> GetByUserAndAppointmentIdAsync(Guid userId, Guid appointmentId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.AppointmentId == appointmentId);
        }

        public async Task<List<Review>> GetByAppointmentIdsAsync(List<Guid> appointmentIds)
        {
            return await _context.Reviews
                .Where(r => appointmentIds.Contains(r.AppointmentId))
                .ToListAsync();
        }

        public async Task<Review?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        }

        public async Task<(int Count, double AverageRating)> GetWorkerReviewStatsAsync(Guid workerId)
        {
            // First, get all appointments handled by this worker
            var appointments = await _context.AppointmentSlots
                .Where(a => a.AssignerWorkerId == workerId)
                .Select(a => a.Id)
                .ToListAsync();

            if (!appointments.Any())
                return (0, 0);

            // Then get appointments that reference these slots
            var appointmentIds = await _context.Appointments
                .Where(a => appointments.Contains(a.AppointmentSlotId) && a.Status == Domain.Entities.Enums.AppointmentStatus.Completed)
                .Select(a => a.Id)
                .ToListAsync();

            if (!appointmentIds.Any())
                return (0, 0);

            // Get reviews for these appointments
            var reviews = await _context.Reviews
                .Where(r => appointmentIds.Contains(r.AppointmentId))
                .ToListAsync();

            int count = reviews.Count;
            double avgRating = count > 0 ? reviews.Average(r => r.Rating) : 0;

            return (count, avgRating);
        }
    }
}
