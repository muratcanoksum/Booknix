using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Booknix.Persistence.Repositories
{
    public class EfUserProfileRepository : IUserProfileRepository
    {
        private readonly BooknixDbContext _context;

        public EfUserProfileRepository(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile> GetByUserIdAsync(Guid userId)
        {
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    BirthDate = null,
                    PhoneNumber = null,
                    ProfileImagePath = null
                };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return profile;
        }


        public async Task AddAsync(UserProfile profile)
        {
            await _context.UserProfiles.AddAsync(profile);
        }

        public Task UpdateAsync(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
