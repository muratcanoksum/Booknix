using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto request, string roleName);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
        Task<RequestResult> VerifyEmailAsync(string token);
        Task<bool> SendPasswordResetTokenAsync(string email);
        Task<bool> CheckTokenExpire(string token);
        Task<RequestResult> ResetPasswordAsync(string token, string newPassword);
        Task<RequestResult> ResetPasswordWithPass(Guid userId, string oldPassword, string newPassword);
    }
}
