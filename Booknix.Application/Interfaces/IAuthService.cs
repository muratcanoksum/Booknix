using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto request, string roleName);
        Task<(AuthResponseDto?, string? Message)> LoginAsync(LoginRequestDto request);
        Task<RequestResult> VerifyEmailAsync(string token);
        Task<bool> SendPasswordResetTokenAsync(string email);
        Task<bool> CheckTokenExpire(string token);
        Task<RequestResult> ResetPasswordAsync(string token, string newPassword);
        Task<RequestResult> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<RequestResult> ChangeEmail(Guid userId, string newEmail);
        Task<(RequestResult, string? NewEmail)> ChangeEmailVerify(Guid userId, string token);
        Task<RequestResult> DeleteAccount(Guid userId, string password);
        Task<RequestResult> DeleteAccountVerify(Guid userId, string token);
        Task<RequestResult> ApproveIpAsync(string token);
    }
}
