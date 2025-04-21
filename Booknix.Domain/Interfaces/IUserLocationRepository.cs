using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface IUserLocationRepository
    {
        Task<UserLocation?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserLocation>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<UserLocation>> GetByLocationIdAsync(Guid locationId);
        Task<UserLocation?> GetByUserAndLocationAsync(Guid userId, Guid locationId);
        Task<bool> ExistsAsync(Guid userId, Guid locationId);

        Task AddAsync(UserLocation userLocation);
        Task UpdateAsync(UserLocation userLocation);
        Task DeleteAsync(Guid id);
    }
}
