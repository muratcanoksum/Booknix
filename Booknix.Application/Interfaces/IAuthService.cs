using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto request, string roleName);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
        Task<VerifyEmailResult> VerifyEmailAsync(string token);
        Task<bool> SendPasswordResetTokenAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
