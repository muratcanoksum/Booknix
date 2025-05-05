using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data; // ← senin BooknixDbContext burada
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfUserSessionRepository(BooknixDbContext context) : IUserSessionRepository
    {
        private readonly BooknixDbContext _context = context;
        private readonly DbSet<UserSession> _sessions = context.Set<UserSession>();

        public async Task DeactivateByIdAsync(Guid sessionId)
        {
            var session = await _sessions.FirstOrDefaultAsync(x => x.Id == sessionId);
            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAsync(UserSession session)
        {
            var exists = await _sessions.AnyAsync(s => s.UserId == session.UserId && s.SessionKey == session.SessionKey && s.IsActive);
            if (exists) return;

            await _sessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        public async Task<UserSession?> GetBySessionKeyAsync(Guid userId, string sessionKey)
        {
            return await _sessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionKey == sessionKey && s.IsActive);
        }

        public async Task DeactivateAllByUserIdAsync(Guid userId)
        {
            var sessions = await _sessions.Where(s => s.UserId == userId && s.IsActive).ToListAsync();
            foreach (var s in sessions)
                s.IsActive = false;

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateBySessionKeyAsync(Guid userId, string sessionKey)
        {
            var session = await _sessions.FirstOrDefaultAsync(s => s.UserId == userId && s.SessionKey == sessionKey && s.IsActive);
            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateLastAccessedAtAsync(Guid userId, string sessionKey)
        {
            var session = await _sessions.FirstOrDefaultAsync(s => s.UserId == userId && s.SessionKey == sessionKey && s.IsActive);
            if (session != null)
            {
                session.LastAccessedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId)
        {
            return await _sessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LastAccessedAt)
                .ToListAsync();
        }
    }
}
