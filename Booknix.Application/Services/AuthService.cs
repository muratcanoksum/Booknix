using Booknix.Application.DTOs;
using Booknix.Application.Helpers;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
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
        private readonly IAuditLogger _auditLogger;

        public AuthService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IEmailSender emailSender,
            IAppSettings appSettings,
            IAuditLogger auditLogger)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _emailSender = emailSender;
            _appSettings = appSettings;
            _auditLogger = auditLogger;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto request, string roleName)
        {
            var existing = await _userRepo.GetByEmailAsync(request.Email);
            if (existing != null)
            {
                await _auditLogger.LogAsync(null, "FailedRegister", "User", null, null, $"E-posta ({request.Email}) zaten kullanılıyor.");
                return false;
            }

            var role = await _roleRepo.GetByNameAsync(roleName);
            if (role == null)
            {
                await _auditLogger.LogAsync(null, "FailedRegister", "User", null, null, $"Tanımsız rol: {roleName}");
                return false;
            }

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
                { "minutes", EmailHelper.TokenExpireMinutes.ToString() }
            });

            try
            {
                await _emailSender.SendEmailAsync(
                     user.Email,
                     "Booknix • E-Posta Doğrulama",
                     htmlBody,
                     "Booknix Doğrulama"
                 );

            }
            catch (Exception ex)
            {
                await _auditLogger.LogAsync(null, "FailedRegister", "User", null, null, $"E-posta gönderilemedi: {ex.Message}");
                return false;
            }

            await _userRepo.AddAsync(user);

            // ✅ Başarılı kayıt logu
            await _auditLogger.LogAsync(user.Id, "Register", "User", user.Id.ToString(), null, "Yeni kullanıcı kaydı tamamlandı");

            return true;
        }

        public async Task<(AuthResponseDto?, string? Message)> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);

            // Kullanıcı yoksa
            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedLogin", "User", null, null, $"Geçersiz e-posta: {request.Email}");
                return (null, "Kullanıcı bulunamadı. Lütfen e-posta adresinizi kontrol ediniz.");
            }

            // Şifre yanlışsa
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await _auditLogger.LogAsync(user.Id, "FailedLogin", "User", user.Id.ToString(), null, "Yanlış şifre ile giriş denemesi");
                return (null, "Şifreniz yanlış. Lütfen tekrar deneyin.");
            }

            // E-posta doğrulanmamışsa
            if (!user.IsEmailConfirmed)
            {
                if (EmailHelper.IsTokenExpired(user.TokenGeneratedAt))
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
                { "minutes", EmailHelper.TokenExpireMinutes.ToString() }
            });

                    await _emailSender.SendEmailAsync(
                        user.Email,
                        "Booknix • E-Posta Doğrulama Linkiniz Yenilendi",
                        htmlBody,
                        "Booknix Doğrulama"
                    );

                }

                await _auditLogger.LogAsync(user.Id, "UnverifiedLoginAttempt", "User", user.Id.ToString(), null, "Doğrulanmamış e-posta ile giriş denemesi");

                return (null, "E-posta adresiniz henüz doğrulanmamış. Yeni doğrulama bağlantısı e-posta adresinize gönderildi.");
            }

            // Başarılı giriş
            await _auditLogger.LogAsync(user.Id, "Login", "User", user.Id.ToString(), null, "Kullanıcı başarılı şekilde giriş yaptı.");

            return (new AuthResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? "Unknown"
            }, "");
        }

        public async Task<RequestResult> VerifyEmailAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                await _auditLogger.LogAsync(null, "FailedEmailVerification", "User", null, null, "Token boş veya geçersiz.");
                return new RequestResult { Success = false, Message = "Token geçersiz." };
            }

            var user = await _userRepo.GetByVerificationTokenAsync(token);
            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedEmailVerification", "User", null, null, "Token ile eşleşen kullanıcı bulunamadı.");
                return new RequestResult { Success = false, Message = "Kullanıcı bulunamadı." };
            }

            if (user.IsEmailConfirmed)
            {
                await _auditLogger.LogAsync(user.Id, "EmailAlreadyVerified", "User", user.Id.ToString(), null, "Kullanıcı zaten e-postasını doğrulamış.");
                return new RequestResult { Success = true, Message = "E-posta zaten doğrulanmış." };
            }

            if (EmailHelper.IsTokenExpired(user.TokenGeneratedAt))
            {
                await _auditLogger.LogAsync(user.Id, "FailedEmailVerification", "User", user.Id.ToString(), null, "Doğrulama token süresi dolmuş.");
                return new RequestResult
                {
                    Success = false,
                    Message = "Doğrulama bağlantısının süresi dolmuş. Lütfen giriş yaparak yeni bağlantı gönderilmesini sağlayın."
                };
            }

            user.IsEmailConfirmed = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EmailVerificationToken = null;
            user.TokenGeneratedAt = null;

            await _userRepo.UpdateAsync(user);

            await _auditLogger.LogAsync(user.Id, "EmailVerified", "User", user.Id.ToString(), null, "Kullanıcı e-postasını doğruladı.");

            return new RequestResult
            {
                Success = true,
                Message = "E-posta başarıyla doğrulandı!"
            };
        }

        public async Task<bool> SendPasswordResetTokenAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || !user.IsEmailConfirmed)
            {
                await _auditLogger.LogAsync(
                    null,
                    "FailedPasswordResetRequest",
                    "User",
                    null,
                    null,
                    $"E-posta bulunamadı veya doğrulanmamış: {email}"
                );
                return false;
            }

            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetRequestedAt = DateTime.UtcNow;

            await _userRepo.UpdateAsync(user);

            var resetLink = $"{_appSettings.BaseUrl}/Auth/ResetPassword?token={token}";

            var html = EmailTemplateHelper.LoadTemplate("PasswordReset", new Dictionary<string, string>
            {
                { "fullname", user.FullName },
                { "resetLink", resetLink },
                { "minutes", EmailHelper.TokenExpireMinutes.ToString() }
            });

            await _emailSender.SendEmailAsync(
                user.Email,
                "Booknix • Şifre Sıfırlama Talebi",
                html,
                "Booknix Güvenlik"
            );


            await _auditLogger.LogAsync(
                user.Id,
                "PasswordResetRequest",
                "User",
                user.Id.ToString(),
                null,
                "Şifre sıfırlama bağlantısı e-posta ile gönderildi."
            );

            return true;
        }

        public async Task<bool> CheckTokenExpire(string token)
        {
            var user = await _userRepo.GetByPasswordResetTokenAsync(token);
            if (user == null)
                return false;
            if (EmailHelper.IsTokenExpired(user.PasswordResetRequestedAt))
                return false;
            return true;
        }

        public async Task<RequestResult> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _userRepo.GetByPasswordResetTokenAsync(token);
            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedPasswordReset", "User", null, null, "Geçersiz token ile şifre sıfırlama denemesi.");
                return new RequestResult { Success = false, Message = "Kullanıcı bulunamadı." };
            }

            if (EmailHelper.IsTokenExpired(user.PasswordResetRequestedAt))
            {
                await _auditLogger.LogAsync(user.Id, "FailedPasswordReset", "User", user.Id.ToString(), null, "Şifre sıfırlama token süresi dolmuş.");
                return new RequestResult { Success = false, Message = "Token süresi dolmuş veya geçersiz." };
            }

            // Mevcut şifre ile yeni şifre aynı ise
            if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            {
                await _auditLogger.LogAsync(user.Id, "FailedPasswordReset", "User", user.Id.ToString(), null, "Yeni şifre mevcut şifre ile aynı.");
                return new RequestResult { Success = false, Message = "Eski şifreniz ile yeni şifreniz aynı olamaz." };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetRequestedAt = null;

            await _userRepo.UpdateAsync(user);

            await _auditLogger.LogAsync(user.Id, "PasswordReset", "User", user.Id.ToString());

            return new RequestResult
            {
                Success = true,
                Message = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz."
            };
        }

        public async Task<RequestResult> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedPasswordChange", "User", null, null, "Kullanıcı bulunamadı.");
                return new RequestResult { Success = false, Message = "Kullanıcı bulunamadı." };
            }

            // Eski şifreyi doğrulama
            var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
            if (!isOldPasswordValid)
            {
                await _auditLogger.LogAsync(user.Id, "FailedPasswordChange", "User", user.Id.ToString(), null, "Mevcut şifre yanlış girildi.");
                return new RequestResult { Success = false, Message = "Mevcut şifreniz doğru değil." };
            }

            // Yeni şifre ile eski şifreyi karşılaştırma
            if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            {
                await _auditLogger.LogAsync(user.Id, "FailedPasswordChange", "User", user.Id.ToString(), null, "Yeni şifre eski şifreyle aynı.");
                return new RequestResult { Success = false, Message = "Eski şifreniz ile yeni şifreniz aynı olamaz." };
            }

            // Yeni şifreyi hash'leyerek kaydetme
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepo.UpdateAsync(user);

            await _auditLogger.LogAsync(user.Id, "PasswordChange", "User", user.Id.ToString());

            return new RequestResult
            {
                Success = true,
                Message = "Şifreniz başarıyla güncellendi."
            };
        }

        public async Task<RequestResult> ChangeEmail(Guid userId, string newEmail)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedEmailChangeRequest", "User", null, null, "Kullanıcı bulunamadı.");
                return new RequestResult { Success = false, Message = "Kullanıcı bulunamadı." };
            }

            var existingUser = await _userRepo.GetByEmailAsync(newEmail);
            if (existingUser != null)
            {
                await _auditLogger.LogAsync(user.Id, "FailedEmailChangeRequest", "User", user.Id.ToString(), null, $"'{newEmail}' adresi zaten kullanımda.");
                return new RequestResult { Success = false, Message = "Bu e-posta adresi zaten kullanılıyor." };
            }

            var random = new Random();
            var verifyCode = random.Next(100000, 999999).ToString();

            user.MailChangeVerifyToken = verifyCode;
            user.MailChangeRequestedAt = DateTime.UtcNow;
            user.PendingEmail = newEmail;

            await _userRepo.UpdateAsync(user);

            var html = EmailTemplateHelper.LoadTemplate("EmailChangeVerify", new Dictionary<string, string>
            {
                { "fullname", user.FullName },
                { "verifyCode", verifyCode },
                { "minutes", EmailHelper.TokenExpireMinutes.ToString() }
            });

            await _emailSender.SendEmailAsync(
                user.Email,
                "Booknix • E-Posta Değişikliği Doğrulama",
                html,
                "Booknix Hesap"
            );

            await _auditLogger.LogAsync(user.Id, "EmailChangeRequest", "User", user.Id.ToString(), null, $"Yeni e-posta adresi için doğrulama kodu gönderildi: {newEmail}");

            return new RequestResult
            {
                Success = true,
                Message = "Mevcut e-posta adresinize gönderilen doğrulama kodunu aşağıdaki alana giriniz. Kod 15 dakika boyunca geçerlidir."
            };
        }

        public async Task<(RequestResult, string? NewEmail)> ChangeEmailVerify(Guid userId, string token)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedEmailChangeConfirm", "User", null, null, "Kullanıcı bulunamadı.");
                return (new RequestResult { Success = false, Message = "Kullanıcı bulunamadı." }, NewEmail: "");
            }

            if (string.IsNullOrEmpty(token))
            {
                await _auditLogger.LogAsync(user.Id, "FailedEmailChangeConfirm", "User", user.Id.ToString(), null, "Token boş.");
                return (new RequestResult { Success = false, Message = "Token geçersiz." }, NewEmail: "");
            }

            if (user.MailChangeVerifyToken != token)
            {
                await _auditLogger.LogAsync(user.Id, "FailedEmailChangeConfirm", "User", user.Id.ToString(), null, "Doğrulama kodu hatalı.");
                return (new RequestResult { Success = false, Message = "Doğrulama kodu geçersiz." }, NewEmail: "");
            }

            if (EmailHelper.IsTokenExpired(user.MailChangeRequestedAt))
            {
                await _auditLogger.LogAsync(user.Id, "FailedEmailChangeConfirm", "User", user.Id.ToString(), null, "Doğrulama kodunun süresi dolmuş.");
                return (new RequestResult { Success = false, Message = "Doğrulama kodunun süresi dolmuş." }, NewEmail: "");
            }

            var html = EmailTemplateHelper.LoadTemplate("EmailChangeAlert", new Dictionary<string, string>
            {
                { "fullname", user.FullName },
                { "newEmail", user.PendingEmail! },
                { "oldEmail", user.Email }
            });

            user.PreviousEmail = user.Email;
            user.EmailChangedAt = DateTime.UtcNow;
            user.Email = user.PendingEmail!;
            user.PendingEmail = null;
            user.MailChangeVerifyToken = null;
            user.MailChangeRequestedAt = null;

            await _userRepo.UpdateAsync(user);

            await _emailSender.SendEmailAsync(
                user.Email,
                "Booknix • E-Posta Adresiniz Güncellendi",
                html,
                "Booknix Güvenlik"
            );

            await _auditLogger.LogAsync(
                user.Id,
                "EmailChange",
                "User",
                user.Id.ToString(),
                null,
                $"E-posta değişikliği tamamlandı. Yeni: {user.Email} Eski: {user.PreviousEmail}"
            );

            return (new RequestResult
            {
                Success = true,
                Message = "E-posta adresiniz başarıyla güncellendi."
            }, NewEmail: user.Email);
        }

        public async Task<RequestResult> DeleteAccount(Guid userId, string password)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedAccountDeletion", "User", null, null, "Kullanıcı bulunamadı.");
                return new RequestResult { Success = false, Message = "Kullanıcı bulunamadı." };
            }

            // Şifreyi doğrulama
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isPasswordValid)
            {
                await _auditLogger.LogAsync(userId, "FailedAccountDeletion", "User", userId.ToString(), null, "Hesap silme işleminde hatalı şifre girildi.");
                return new RequestResult { Success = false, Message = "Şifre geçersiz." };
            }

            // Token oluşturma
            var random = new Random();
            var verifyCode = random.Next(100000, 999999).ToString();

            user.DeleteToken = verifyCode;
            user.DeleteTokenRequesAt = DateTime.UtcNow;

            var html = EmailTemplateHelper.LoadTemplate("DeleteAccountVerify", new Dictionary<string, string>
            {
                { "fullname", user.FullName },
                { "verifyCode", verifyCode },
                { "minutes", EmailHelper.TokenExpireMinutes.ToString() }
            });

            await _emailSender.SendEmailAsync(
                user.Email,
                "Booknix • Hesap Silme Talebi",
               html,
                "Booknix Güvenlik"
            );

            await _auditLogger.LogAsync(userId, "AccountDeletionRequested", "User", userId.ToString(), null, "Hesap silme doğrulama kodu gönderildi.");

            return new RequestResult
            {
                Success = true,
                Message = "Mevcut e-posta adresinize gönderilen doğrulama kodunu aşağıdaki alana giriniz. Kod 15 dakika boyunca geçerlidir."
            };
        }

        public async Task<RequestResult> DeleteAccountVerify(Guid userId, string token)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedAccountDeletion", "User", null, null, "Kullanıcı bulunamadı.");
                return new RequestResult { Success = false, Message = "Kullanıcı bulunamadı." };
            }

            if (string.IsNullOrEmpty(token))
            {
                await _auditLogger.LogAsync(userId, "FailedAccountDeletion", "User", userId.ToString(), null, "Token boş veya null gönderildi.");
                return new RequestResult { Success = false, Message = "Token geçersiz." };
            }

            if (user.DeleteToken != token)
            {
                await _auditLogger.LogAsync(userId, "FailedAccountDeletion", "User", userId.ToString(), null, "Geçersiz doğrulama kodu girildi.");
                return new RequestResult { Success = false, Message = "Doğrulama kodu geçersiz." };
            }

            if (EmailHelper.IsTokenExpired(user.DeleteTokenRequesAt))
            {
                await _auditLogger.LogAsync(userId, "FailedAccountDeletion", "User", userId.ToString(), null, "Doğrulama kodunun süresi dolmuş.");
                return new RequestResult { Success = false, Message = "Doğrulama kodunun süresi dolmuş." };
            }

            await _auditLogger.LogAsync(userId, "AccountDeleted", "User", userId.ToString(), null, "Kullanıcı hesabı başarıyla silindi.");
            await _userRepo.DeleteAsync(user);

            return new RequestResult
            {
                Success = true,
                Message = "Hesabınız başarıyla silindi."
            };
        }

    }
}
