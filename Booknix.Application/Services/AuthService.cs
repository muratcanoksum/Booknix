using Booknix.Application.DTOs;
using Booknix.Application.Helpers;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Shared.Helpers;
using Booknix.Shared.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Booknix.Application.Services
{
    public class AuthService(
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IEmailSender emailSender,
        IAuditLogger auditLogger,
        ITrustedIpRepository trustedIpRepo,
        IWorkerRepository workerRepo,
        IHttpContextAccessor httpContextAccessor
            ) : IAuthService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IAuditLogger _auditLogger = auditLogger;
        private readonly ITrustedIpRepository _trustedIpRepo = trustedIpRepo;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IWorkerRepository _workerRepo = workerRepo;

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

            var verifyLink = $"{EmailHelper.BaseUrl}/Auth/VerifyEmail?token={token}";

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
            var (user, validationMessage) = await ValidateUserCredentialsAsync(request);
            if (validationMessage != null)
                return (null, validationMessage);

            if (user!.Role?.Name == "Admin")
            {
                var ipCheckMessage = await HandleAdminIpCheckAsync(user);
                if (ipCheckMessage != null)
                    return (null, ipCheckMessage);
            }

            await _auditLogger.LogAsync(user.Id, "Login", "User", user.Id.ToString(), null, "Kullanıcı başarılı şekilde giriş yaptı.");
            var worker = await _workerRepo.GetByUserIdAsync(user.Id); // Kullanıcıyı yükle

            return (new AuthResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? "Unknown",
                LocationId = worker?.LocationId,
                LocationRole = worker?.RoleInLocation
            }, "");
        }

        // Yardımcı Fonksiyonlar

        private async Task<(User?, string?)> ValidateUserCredentialsAsync(LoginRequestDto request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user == null)
            {
                await _auditLogger.LogAsync(null, "FailedLogin", "User", null, null, $"Geçersiz e-posta: {request.Email}");
                return (null, "Kullanıcı bulunamadı. Lütfen e-posta adresinizi kontrol ediniz.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await _auditLogger.LogAsync(user.Id, "FailedLogin", "User", user.Id.ToString(), null, "Yanlış şifre ile giriş denemesi");
                return (null, "Şifreniz yanlış. Lütfen tekrar deneyin.");
            }

            if (!user.IsEmailConfirmed)
            {
                if (EmailHelper.IsTokenExpired(user.TokenGeneratedAt))
                {
                    var newToken = Guid.NewGuid().ToString();
                    user.EmailVerificationToken = newToken;
                    user.TokenGeneratedAt = DateTime.UtcNow;
                    await _userRepo.UpdateAsync(user);

                    var verifyLink = $"{EmailHelper.BaseUrl}/Auth/VerifyEmail?token={newToken}";
                    var htmlBody = EmailTemplateHelper.LoadTemplate("EmailVerification", new Dictionary<string, string>
            {
                { "fullname", user.FullName },
                { "verifyLink", verifyLink },
                { "minutes", EmailHelper.TokenExpireMinutes.ToString() }
            });

                    await _emailSender.SendEmailAsync(user.Email, "Booknix • E-Posta Doğrulama Linkiniz Yenilendi", htmlBody, "Booknix Doğrulama");
                }

                await _auditLogger.LogAsync(user.Id, "UnverifiedLoginAttempt", "User", user.Id.ToString(), null, "Doğrulanmamış e-posta ile giriş denemesi");
                return (null, "E-posta adresiniz henüz doğrulanmamış. Yeni doğrulama bağlantısı gönderildi.");
            }

            return (user, null);
        }

        private async Task<string?> HandleAdminIpCheckAsync(User user)
        {
            var currentIp = IpHelper.GetCurrentIp(_httpContextAccessor);
            var trustedIp = await _trustedIpRepo.GetByUserAndIpAsync(user.Id, currentIp);

            if (trustedIp != null && trustedIp.IsApproved)
                return null;

            var token = Guid.NewGuid().ToString();

            if (trustedIp == null)
            {
                await _trustedIpRepo.AddAsync(new TrustedIp
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    IpAddress = currentIp,
                    IsApproved = false,
                    RequestedAt = DateTime.UtcNow,
                    Token = token
                });

                await _auditLogger.LogAsync(user.Id, "UntrustedIpDetected", "TrustedIp", user.Id.ToString(), currentIp, "Yeni IP adresi güvenilir IP'lere eklendi ve onay bekliyor.");
            }
            else
            {
                trustedIp.Token = token;

                await _trustedIpRepo.UpdateAsync(trustedIp);

                await _auditLogger.LogAsync(user.Id, "UnapprovedIpLoginAttempt", "TrustedIp", user.Id.ToString(), currentIp, "Onaylanmamış IP adresinden giriş denemesi yapıldı.");
            }

            var role = await _roleRepo.GetByNameAsync("Admin");
            var admins = role == null ? [user] : await _userRepo.GetUsersByRoleIdAsync(role.Id);
            if (admins.Count == 0) admins.Add(user);

            var approvalUrl = $"{EmailHelper.BaseUrl}/Auth/ApproveIp?token={token}";
            var html = EmailTemplateHelper.LoadTemplate("NewIpApproval", new Dictionary<string, string>
            {
                { "fullname", user.FullName },
                { "currentIp", currentIp },
                { "approvalUrl", approvalUrl },
                { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            });

            foreach (var admin in admins)
            {
                await _emailSender.SendEmailAsync(admin.Email, "Booknix • Yeni IP Girişi Tespiti", html, "Booknix Yönetim");
            }

            await _auditLogger.LogAsync(user.Id, "IpApprovalEmailSent", "TrustedIp", user.Id.ToString(), currentIp, "Yeni IP adresi için onay e-postası gönderildi.");

            return "Yeni bir IP adresinden giriş algılandı. Onay maili gönderildi.";
        }

        // Yardımcı Fonksiyonlar

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

            var resetLink = $"{EmailHelper.BaseUrl}/Auth/ResetPassword?token={token}";

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

        public async Task<RequestResult> ApproveIpAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                await _auditLogger.LogAsync(null, "FailedIpApproval", "TrustedIp", null, null, "IP onay işlemi başarısız oldu: Token değeri boş.");
                return new RequestResult { Success = false, Message = "Geçersiz doğrulama bağlantısı. Lütfen tekrar deneyin." };
            }

            var trustedIp = await _trustedIpRepo.GetByVerificationTokenAsync(token);
            if (trustedIp == null)
            {
                await _auditLogger.LogAsync(null, "FailedIpApproval", "TrustedIp", null, null, "IP onay işlemi başarısız oldu: Token ile eşleşen kayıt bulunamadı.");
                return new RequestResult { Success = false, Message = "IP adresi doğrulaması başarısız. Geçersiz veya süresi dolmuş bağlantı." };
            }

            if (trustedIp.IsApproved)
            {
                await _auditLogger.LogAsync(trustedIp.UserId, "RedundantIpApproval", "TrustedIp", trustedIp.Id.ToString(), trustedIp.IpAddress, "IP adresi zaten onaylıydı.");
                return new RequestResult { Success = true, Message = "Bu IP adresi zaten daha önce onaylanmış." };
            }

            trustedIp.IsApproved = true;
            trustedIp.ApprovedAt = DateTime.UtcNow;
            trustedIp.Token = string.Empty;

            await _trustedIpRepo.UpdateAsync(trustedIp);
            await _auditLogger.LogAsync(trustedIp.UserId, "IpApproved", "TrustedIp", trustedIp.Id.ToString(), trustedIp.IpAddress, "Yeni IP adresi başarıyla onaylandı.");

            return new RequestResult
            {
                Success = true,
                Message = "IP adresi başarıyla onaylandı. Artık bu adres üzerinden yönetim paneline erişebilirsiniz."
            };
        }


    }
}
