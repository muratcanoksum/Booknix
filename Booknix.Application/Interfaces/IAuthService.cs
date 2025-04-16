using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto request, string roleName);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    }
}
