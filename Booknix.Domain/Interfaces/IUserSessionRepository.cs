using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IUserSessionRepository
    {
        Task AddAsync(UserSession session);
        Task<UserSession?> GetBySessionKeyAsync(Guid userId, string sessionKey);
        Task DeactivateAllByUserIdAsync(Guid userId);
        Task UpdateLastAccessedAtAsync(Guid userId, string sessionKey);
        Task<List<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId);
        Task DeactivateBySessionKeyAsync(Guid userId, string sessionKey);
        Task ExtendSessionExpirationAsync(Guid userId, string sessionKey, DateTime newExpiresAt);

    }
}
