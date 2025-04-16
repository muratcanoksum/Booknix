using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;

        public AuthService(IUserRepository userRepo, IRoleRepository roleRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request, string roleName)
        {
            var existing = await _userRepo.GetByEmailAsync(request.Email);
            if (existing != null) return false;

            var role = await _roleRepo.GetByNameAsync(roleName);
            if (role == null) return false;

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = role.Id
            };

            await _userRepo.AddAsync(user);
            return true;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user == null) return null;

            var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValid) return null;

            return new AuthResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? "Unknown"
            };
        }
    }
}
