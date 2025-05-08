using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfEmailQueueRepository : IEmailQueueRepository
    {
        private readonly BooknixDbContext _context;

        public EfEmailQueueRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(EmailQueue email)
        {
            _context.EmailQueues.Add(email);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EmailQueue>> GetPendingAsync(int limit = 10)
        {
            return await _context.EmailQueues
                .Where(e => e.Status == EmailQueueStatus.Pending)
                .OrderBy(e => e.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task UpdateAsync(EmailQueue email)
        {
            _context.EmailQueues.Update(email);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EmailQueue>> GetAllAsync()
        {
            return await _context.EmailQueues
                .OrderByDescending(e => e.UpdatedAt)
                .ToListAsync();
        }

        public async Task<List<EmailQueue>> GetByStatusAsync(EmailQueueStatus status)
        {
            return await _context.EmailQueues
                .Where(e => e.Status == status)
                .OrderByDescending(e => e.UpdatedAt)
                .ToListAsync();
        }

        public async Task<EmailQueue?> GetByIdAsync(Guid id)
        {
            return await _context.EmailQueues.FindAsync(id);
        }

    }
}
