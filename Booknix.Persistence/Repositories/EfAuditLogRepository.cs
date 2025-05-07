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

        public async Task<List<AuditLog>> GetLogsByEntityIdAsync(string entityId)
        {
            return await _context.Set<AuditLog>()
                .Where(l => l.EntityId == entityId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }
    }

}
