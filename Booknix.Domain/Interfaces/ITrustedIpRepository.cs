using Booknix.Domain.Entities;

namespace Booknix.Domain.Interfaces
{
    public interface ITrustedIpRepository
    {
        Task<TrustedIp?> GetByUserAndIpAsync(Guid userId, string ip);
        Task AddAsync(TrustedIp trustedIp);
        Task SaveChangesAsync();
        Task UpdateAsync(TrustedIp trustedIp);
        Task<TrustedIp?> GetByVerificationTokenAsync(string token);

    }
}
