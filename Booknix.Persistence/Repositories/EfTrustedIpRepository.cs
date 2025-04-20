using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfTrustedIpRepository(BooknixDbContext context) : ITrustedIpRepository
    {
        private readonly BooknixDbContext _context = context;

        public async Task<TrustedIp?> GetByUserAndIpAsync(Guid userId, string ip)
        {
            return await _context.TrustedIps
                .FirstOrDefaultAsync(t => t.UserId == userId && t.IpAddress == ip);
        }

        public async Task<TrustedIp?> GetByVerificationTokenAsync(string token)
        {
            return await _context.TrustedIps
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task AddAsync(TrustedIp trustedIp)
        {
            await _context.TrustedIps.AddAsync(trustedIp);

            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TrustedIp trustedIp)
        {
            _context.TrustedIps.Update(trustedIp);
            await _context.SaveChangesAsync();
        }

    }

}
