using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfAuditLogRepository : IAuditLogRepository
    {
        private readonly BooknixDbContext _context;

        public EfAuditLogRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetLogsByUserIdAsync(Guid userId)
        {
            return await _context.Set<AuditLog>()
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }
    }
}
