using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(Guid userId);
        Task AddAsync(UserProfile profile);
        Task UpdateAsync(UserProfile profile);
        Task SaveChangesAsync();
    }
}
