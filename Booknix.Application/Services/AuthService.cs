using Booknix.Application.DTOs;
using Booknix.Application.Helpers;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Infrastructure.Email;
using Booknix.Shared.Helpers;
using Booknix.Shared.Interfaces;

namespace Booknix.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IEmailSender _emailSender;
        private readonly IAppSettings _appSettings;

        public AuthService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IEmailSender emailSender,
            IAppSettings appSettings)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _emailSender = emailSender;
            _appSettings = appSettings;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request, string roleName)
        {
            var existing = await _userRepo.GetByEmailAsync(request.Email);
            if (existing != null)
                return false;

            var role = await _roleRepo.GetByNameAsync(roleName);
            if (role == null)
                return false;

            var token = Guid.NewGuid().ToString();

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = role.Id,
                EmailVerificationToken = token,
                TokenGeneratedAt = DateTime.UtcNow,
                IsEmailConfirmed = false
            };

            var verifyLink = $"{_appSettings.BaseUrl}/Auth/VerifyEmail?token={token}";

            var htmlBody = EmailTemplateHelper.LoadTemplate("EmailVerification", new Dictionary<string, string>
            {
                { "fullname", user.FullName },
                { "verifyLink", verifyLink },
                { "minutes", EmailVerificationHelper.TokenExpireMinutes.ToString() }
            });

            try
            {
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Booknix | Email Adresinizi Doğrulayın",
                    htmlBody
                );
            }
            catch (Exception)
            {
                // loglama yapılabilir
                return false;
            }

            await _userRepo.AddAsync(user);
            return true;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            if (!user.IsEmailConfirmed)
            {
                if (EmailVerificationHelper.IsTokenExpired(user.TokenGeneratedAt))
                {
                    var newToken = Guid.NewGuid().ToString();
                    user.EmailVerificationToken = newToken;
                    user.TokenGeneratedAt = DateTime.UtcNow;

                    await _userRepo.UpdateAsync(user);

                    var verifyLink = $"{_appSettings.BaseUrl}/Auth/VerifyEmail?token={newToken}";

                    var htmlBody = EmailTemplateHelper.LoadTemplate("EmailVerification", new Dictionary<string, string>
                    {
                        { "fullname", user.FullName },
                        { "verifyLink", verifyLink },
                        { "minutes", EmailVerificationHelper.TokenExpireMinutes.ToString() }
                    });

                    await _emailSender.SendEmailAsync(
                        user.Email,
                        "Booknix | E-Posta Doğrulama Linkiniz Yenilendi",
                        htmlBody
                    );
                }

                return new AuthResponseDto
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = "Unverified"
                };
            }

            return new AuthResponseDto
            {
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? "Unknown"
            };
        }

        public async Task<VerifyEmailResult> VerifyEmailAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return new VerifyEmailResult { Success = false, Message = "Token geçersiz." };

            var user = await _userRepo.GetByVerificationTokenAsync(token);
            if (user == null)
                return new VerifyEmailResult { Success = false, Message = "Kullanıcı bulunamadı." };

            if (user.IsEmailConfirmed)
                return new VerifyEmailResult { Success = true, Message = "E-posta zaten doğrulanmış." };

            if (EmailVerificationHelper.IsTokenExpired(user.TokenGeneratedAt))
            {
                return new VerifyEmailResult
                {
                    Success = false,
                    Message = "Doğrulama bağlantısının süresi dolmuş. Lütfen giriş yaparak yeni bağlantı isteyin."
                };
            }

            user.IsEmailConfirmed = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EmailVerificationToken = null;
            user.TokenGeneratedAt = null;

            await _userRepo.UpdateAsync(user);

            return new VerifyEmailResult
            {
                Success = true,
                Message = "E-posta başarıyla doğrulandı!"
            };
        }

        public async Task<bool> SendPasswordResetTokenAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || !user.IsEmailConfirmed)
                return false;

            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetRequestedAt = DateTime.UtcNow;

            await _userRepo.UpdateAsync(user);

            var resetLink = $"{_appSettings.BaseUrl}/Auth/ResetPassword?token={token}";

            var html = EmailTemplateHelper.LoadTemplate("PasswordReset", new Dictionary<string, string>
    {
        { "fullname", user.FullName },
        { "resetLink", resetLink },
        { "minutes", EmailVerificationHelper.TokenExpireMinutes.ToString() }
    });

            await _emailSender.SendEmailAsync(user.Email, "Booknix | Şifre Sıfırlama", html);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _userRepo.GetByPasswordResetTokenAsync(token);
            if (user == null)
                return false;

            if (EmailVerificationHelper.IsTokenExpired(user.PasswordResetRequestedAt))
                return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetRequestedAt = null;

            await _userRepo.UpdateAsync(user);
            return true;
        }


    }
}
