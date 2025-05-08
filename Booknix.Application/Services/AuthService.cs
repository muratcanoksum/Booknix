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
        IHttpContextAccessor httpContextAccessor,
        IUserSessionRepository userSessionRepo
            ) : IAuthService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IAuditLogger _auditLogger = auditLogger;
        private readonly ITrustedIpRepository _trustedIpRepo = trustedIpRepo;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IWorkerRepository _workerRepo = workerRepo;
        private readonly IUserSessionRepository _userSessionRepo = userSessionRepo;

        public async Task<bool> RegisterAsync(RegisterRequestDto request, string roleName)
        {
            var existing = await _userRepo.GetByEmailAsync(request.Email);
            if (existing != null)
            {
                await _auditLogger.LogAsync(null, "FailedRegister", "Auth/Register", null, null, null, $"E-posta ({request.Email}) zaten kullanılıyor.");
                return false;
            }

            var role = await _roleRepo.GetByNameAsync(roleName);
            if (role == null)
            {
                await _auditLogger.LogAsync(null, "FailedRegister", "Auth/Register", null, null, null, $"Tanımsız rol: {roleName}");
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
                await _userRepo.AddAsync(user);

                await _emailSender.SendEmailAsync(
                     user.Email,
                     "Booknix • E-Posta Doğrulama",
                     htmlBody,
                     "Booknix Doğrulama"
                 );

            }
            catch (Exception ex)
            {
                await _auditLogger.LogAsync(
                    null,
                    "FailedRegister",
                    "Auth/Register",
                    null,
                    null,
                    null,
                    $"Kayıt oluşturulurken hata oluştu. E-posta: {request.Email}, Hata: {ex.Message}"
                );
                return false;
            }

            // ✅ Başarılı kayıt logu
            await _auditLogger.LogAsync(user.Id, "Register", "Auth/Register", null, null, null, "Yeni kullanıcı kaydı tamamlandı");

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

            var sessionKey = await CreateUserSessionAsync(user, request.RememberMe);

            await _auditLogger.LogAsync(
                  user.Id,
                  "Login",
                  "Auth/Login",
                  null,
                  sessionKey,
                  null,
                  "Hesabınıza başarılı şekilde giriş yaptınız."
              );

            var worker = await _workerRepo.GetByUserIdAsync(user.Id); // Kullanıcıyı yükle

            return (new AuthResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? "Unknown",
                LocationId = worker?.LocationId,
                LocationRole = worker?.RoleInLocation,
                SessionKey = sessionKey,
            }, "");
        }

        // Yardımcı Fonksiyonlar

        private async Task<(User?, string?)> ValidateUserCredentialsAsync(LoginRequestDto request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user == null)
            {
                await _auditLogger.LogAsync(
                    null,
                    "FailedLogin",
                    "Auth/Login",
                    null,
                    null,
                    null,
                    $"Geçersiz e-posta ile giriş denemesi: {request.Email}"
                );
                return (null, "Kullanıcı bulunamadı. Lütfen e-posta adresinizi kontrol edin.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedLogin",
                    "Auth/Login",
                    null,
                    null,
                    null,
                    "Hesabınızla yanlış şifre kullanılarak bir giriş denemesi yapıldı."
                );

                return (null, "Şifre hatalı. Lütfen tekrar deneyin.");
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

                await _auditLogger.LogAsync(
                     user.Id,
                     "UnverifiedLoginAttempt",
                     "Auth/Login",
                     null,
                     null,
                     null,
                     "Doğrulanmamış e-posta adresinizle bir giriş denemesi yapıldı. Yeni doğrulama bağlantısı e-posta adresinize gönderildi."
                 );


                return (null, "E-posta adresiniz henüz doğrulanmamış. Yeni bir doğrulama bağlantısı gönderildi.");
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

                await _auditLogger.LogAsync(
                    user.Id,
                    "UntrustedIpDetected",
                    "Auth/Login",
                    null,
                    null,
                    currentIp,
                    "Yeni bir IP adresinden hesabınıza giriş yapılmak istendi. Bu adres onay için beklemeye alındı."
                );

            }
            else
            {
                trustedIp.Token = token;

                await _trustedIpRepo.UpdateAsync(trustedIp);

                await _auditLogger.LogAsync(
                    user.Id,
                    "UnapprovedIpLoginAttempt",
                    "Auth/Login",
                    null,
                    null,
                    currentIp,
                    "Daha önce tanımladığınız ancak henüz onaylanmamış bir IP adresinden hesabınıza giriş yapılmak istendi."
                );

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

            await _auditLogger.LogAsync(
                user.Id,
                "IpApprovalEmailSent",
                "Auth/Login",
                null,
                null,
                currentIp,
                "Yeni IP adresini onaylamanız için e-posta gönderildi."
            );


            return "Yeni bir IP adresinden giriş algılandı. Onay e-postası gönderildi.";
        }

        private async Task<string> CreateUserSessionAsync(User user, bool rememberMe)
        {
            var context = _httpContextAccessor.HttpContext!;
            var request = context.Request;
            var response = context.Response;

            string sessionKey;
            var expiresAt = rememberMe
                ? DateTime.UtcNow.AddDays(30)
                : DateTime.UtcNow.AddHours(1);

            // 1. SessionKey Cookie kontrolü
            if (request.Cookies.ContainsKey("PersistentSessionKey"))
            {
                sessionKey = request.Cookies["PersistentSessionKey"]!;

                // rememberMe aktifse cookie süresini uzat
                if (rememberMe)
                {
                    response.Cookies.Append("PersistentSessionKey", sessionKey, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(30),
                        HttpOnly = true,
                        IsEssential = true
                    });

                    await _userSessionRepo.ExtendSessionExpirationAsync(user.Id, sessionKey, DateTime.UtcNow.AddDays(30));

                }

                await _auditLogger.LogAsync(
                     user.Id,
                     "SessionResumed",
                     "Auth/Login",
                     null,
                     sessionKey,
                     null,
                     "Tarayıcınızdaki mevcut oturum bilgileriyle giriş yapıldı."
                 );

            }
            else
            {
                sessionKey = Guid.NewGuid().ToString();

                response.Cookies.Append("PersistentSessionKey", sessionKey, new CookieOptions
                {
                    Expires = expiresAt,
                    HttpOnly = true,
                    IsEssential = true
                });

                await _auditLogger.LogAsync(
                     user.Id,
                     "SessionCreated",
                     "Auth/Login",
                     null,
                     sessionKey,
                     null,
                     "Hesabınız için yeni bir oturum başlatıldı."
                 );

            }

            // 2. IP / User-Agent bilgisi al
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var ua = request.Headers["User-Agent"].ToString();

            // 3. UserSession tablosuna kaydet
            await _userSessionRepo.AddAsync(new UserSession
            {
                UserId = user.Id,
                SessionKey = sessionKey,
                IpAddress = ip,
                UserAgent = ua,
                ExpiresAt = expiresAt // ✅ önemli kısım
            });

            return sessionKey;
        }

        // Yardımcı Fonksiyonlar

        public async Task<RequestResult> VerifyEmailAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                await _auditLogger.LogAsync(
                    null,
                    "FailedEmailVerification",
                    "Auth/VerifyEmail",
                    null,
                    null,
                    null,
                    "E-posta doğrulama işlemi başarısız oldu: Token değeri boş veya geçersiz."
                );
                return new RequestResult { Success = false, Message = "Geçersiz doğrulama kodu." };
            }

            var user = await _userRepo.GetByVerificationTokenAsync(token);
            if (user == null)
            {
                await _auditLogger.LogAsync(
                    null,
                    "FailedEmailVerification",
                    "Auth/VerifyEmail",
                    null,
                    null,
                    null,
                    "E-posta doğrulama işlemi başarısız oldu: Token ile eşleşen kullanıcı bulunamadı."
                );
                return new RequestResult { Success = false, Message = "Kullanıcıya ulaşılamadı." };
            }

            if (user.IsEmailConfirmed)
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "EmailAlreadyVerified",
                    "Auth/VerifyEmail",
                    null,
                    null,
                    null,
                    "E-posta adresiniz zaten daha önce doğrulanmış."
                );
                return new RequestResult { Success = true, Message = "E-posta adresiniz zaten doğrulanmış." };
            }

            if (EmailHelper.IsTokenExpired(user.TokenGeneratedAt))
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedEmailVerification",
                    "Auth/VerifyEmail",
                    null,
                    null,
                    null,
                    "E-posta doğrulama bağlantınızın süresi dolmuş."
                );
                return new RequestResult
                {
                    Success = false,
                    Message = "Doğrulama bağlantısının süresi dolmuş. Lütfen giriş yaparak yeni bir bağlantı talep edin."
                };
            }

            user.IsEmailConfirmed = true;
            user.EmailVerifiedAt = DateTime.UtcNow;
            user.EmailVerificationToken = null;
            user.TokenGeneratedAt = null;

            await _userRepo.UpdateAsync(user);

            await _auditLogger.LogAsync(
                user.Id,
                "EmailVerified",
                "Auth/VerifyEmail",
                null,
                null,
                null,
                "E-posta adresiniz başarıyla doğrulandı."
            );

            return new RequestResult
            {
                Success = true,
                Message = "E-posta adresiniz başarıyla doğrulandı."
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
                    "Auth/ForgotPassword",
                    null,
                    null,
                    null,
                    $"Şifre sıfırlama isteği başarısız: E-posta bulunamadı veya doğrulanmamış ({email})"
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
                "Auth/ForgotPassword",
                null,
                null,
                null,
                "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi."
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
                await _auditLogger.LogAsync(
                    null,
                    "FailedPasswordReset",
                    "Auth/ResetPassword",
                    null,
                    null,
                    null,
                    "Geçersiz veya tanımsız bir bağlantı ile şifre sıfırlama denemesi yapıldı."
                );
                return new RequestResult { Success = false, Message = "Kullanıcı bulunamadı. Lütfen bilgilerinizi kontrol edin." };
            }

            if (EmailHelper.IsTokenExpired(user.PasswordResetRequestedAt))
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedPasswordReset",
                    "Auth/ResetPassword",
                    null,
                    null,
                    null,
                    "Şifre sıfırlama bağlantınızın süresi dolmuş."
                );
                return new RequestResult { Success = false, Message = "Doğrulama kodu geçersiz veya süresi dolmuş." };
            }

            // Mevcut şifre ile yeni şifre aynı ise
            if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedPasswordReset",
                    "Auth/ResetPassword",
                    null,
                    null,
                    null,
                    "Yeni şifreniz mevcut şifrenizle aynı olamaz."
                );
                return new RequestResult { Success = false, Message = "Yeni şifreniz, mevcut şifrenizle aynı olamaz." };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetRequestedAt = null;

            await _userRepo.UpdateAsync(user);

            await _auditLogger.LogAsync(
                user.Id,
                "PasswordReset",
                "Auth/ResetPassword",
                null,
                null,
                null,
                "Şifreniz başarıyla sıfırlandı."
            );

            return new RequestResult
            {
                Success = true,
                Message = "Şifreniz başarıyla güncellendi. Giriş yaparak devam edebilirsiniz."
            };
        }

        public async Task<RequestResult> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                await _auditLogger.LogAsync(
                    null,
                    "FailedPasswordChange",
                    "Auth/ChangePassword",
                    null,
                    null,
                    null,
                    "Şifre değiştirme işlemi başarısız oldu: Kullanıcı bulunamadı."
                );
                return new RequestResult { Success = false, Message = "İlgili kullanıcıya ulaşılamadı." };
            }

            // Eski şifreyi doğrulama
            var isOldPasswordValid = BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash);
            if (!isOldPasswordValid)
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedPasswordChange",
                    "Auth/ChangePassword",
                    null,
                    null,
                    null,
                    "Şifrenizi değiştirme girişiminiz başarısız oldu: Mevcut şifre yanlış girildi."
                );
                return new RequestResult { Success = false, Message = "Mevcut şifreniz hatalı. Lütfen tekrar deneyin." };
            }

            // Yeni şifre ile eski şifreyi karşılaştırma
            if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedPasswordChange",
                    "Auth/ChangePassword",
                    null,
                    null,
                    null,
                    "Yeni şifreniz mevcut şifrenizle aynı olamaz."
                );
                return new RequestResult { Success = false, Message = "Yeni şifreniz, eski şifrenizle aynı olamaz." };
            }

            // Yeni şifreyi hash'leyerek kaydetme
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepo.UpdateAsync(user);

            await _auditLogger.LogAsync(
                user.Id,
                "PasswordChange",
                "Auth/ChangePassword",
                null,
                null,
                null,
                "Şifreniz başarıyla değiştirildi."
            );

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
                await _auditLogger.LogAsync(
                    null,
                    "FailedEmailChangeRequest",
                    "Auth/ChangeEmail",
                    null,
                    null,
                    null,
                    "E-posta değiştirme işlemi başarısız oldu: Kullanıcı bulunamadı."
                );
                return new RequestResult { Success = false, Message = "Kullanıcı sistemde kayıtlı değil." };
            }

            var existingUser = await _userRepo.GetByEmailAsync(newEmail);
            if (existingUser != null)
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedEmailChangeRequest",
                    "Auth/ChangeEmail",
                    null,
                    null,
                    null,
                    $"'{newEmail}' e-posta adresi zaten kullanımda."
                );
                return new RequestResult { Success = false, Message = "Bu e-posta adresi zaten kullanılmakta." };
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

            await _auditLogger.LogAsync(
                user.Id,
                "EmailChangeRequest",
                "Auth/ChangeEmail",
                null,
                null,
                null,
                $"'{newEmail}' adresine doğrulama kodu gönderildi."
            );

            return new RequestResult
            {
                Success = true,
                Message = "Mevcut e-posta adresinize gönderilen doğrulama kodunu aşağıdaki alana giriniz. Kod, 15 dakika boyunca geçerlidir."
            };
        }

        public async Task<(RequestResult, string? NewEmail)> ChangeEmailVerify(Guid userId, string token)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                await _auditLogger.LogAsync(
                    null,
                    "FailedEmailChangeConfirm",
                    "Auth/ChangeEmailVerify",
                    null,
                    null,
                    null,
                    "E-posta değişikliği doğrulama işlemi başarısız oldu: Kullanıcı bulunamadı."
                );
                return (new RequestResult { Success = false, Message = "Kullanıcıya ulaşılamadı." }, NewEmail: "");
            }

            if (string.IsNullOrEmpty(token))
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedEmailChangeConfirm",
                    "Auth/ChangeEmailVerify",
                    null,
                    null,
                    null,
                    "E-posta değişikliği doğrulama kodu boş gönderildi."
                );
                return (new RequestResult { Success = false, Message = "Geçersiz doğrulama kodu." }, NewEmail: "");
            }

            if (user.MailChangeVerifyToken != token)
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedEmailChangeConfirm",
                    "Auth/ChangeEmailVerify",
                    null,
                    null,
                    null,
                    "Girilen e-posta doğrulama kodu geçersiz."
                );
                return (new RequestResult { Success = false, Message = "Doğrulama kodu hatalı veya süresi dolmuş." }, NewEmail: "");
            }

            if (EmailHelper.IsTokenExpired(user.MailChangeRequestedAt))
            {
                await _auditLogger.LogAsync(
                    user.Id,
                    "FailedEmailChangeConfirm",
                    "Auth/ChangeEmailVerify",
                    null,
                    null,
                    null,
                    "E-posta doğrulama kodunun süresi dolmuş."
                );
                return (new RequestResult { Success = false, Message = "Doğrulama kodunun geçerlilik süresi dolmuş." }, NewEmail: "");
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
                "Auth/ChangeEmailVerify",
                null,
                null,
                null,
                $"E-posta adresiniz başarıyla güncellendi. Yeni: {user.Email}, Eski: {user.PreviousEmail}"
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
                await _auditLogger.LogAsync(
                    null,
                    "FailedAccountDeletion",
                    "Auth/Delete",
                    null,
                    null,
                    null,
                    "Hesap silme işlemi başarısız oldu: Kullanıcı bulunamadı."
                );
                return new RequestResult { Success = false, Message = "Kullanıcıya ulaşılamadı." };
            }

            // Şifreyi doğrulama
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!isPasswordValid)
            {
                await _auditLogger.LogAsync(
                    userId,
                    "FailedAccountDeletion",
                    "Auth/Delete",
                    null,
                    null,
                    null,
                    "Hesap silme işlemi sırasında yanlış şifre girildi."
                );
                return new RequestResult { Success = false, Message = "Şifre geçerli değil. Lütfen kontrol edin." };
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

            await _auditLogger.LogAsync(
                userId,
                "AccountDeletionRequested",
                "Auth/Delete",
                null,
                null,
                null,
                "Hesabınızı silmek için doğrulama kodu e-posta adresinize gönderildi."
            );

            return new RequestResult
            {
                Success = true,
                Message = "Mevcut e-posta adresinize gönderilen doğrulama kodunu aşağıya giriniz. Kod 15 dakika süreyle geçerlidir."
            };
        }

        public async Task<RequestResult> DeleteAccountVerify(Guid userId, string token)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                await _auditLogger.LogAsync(
                    null,
                    "FailedAccountDeletion",
                    "Auth/DeleteVerify",
                    null,
                    null,
                    null,
                    "Hesap silme işlemi başarısız oldu: Kullanıcı bulunamadı."
                );

                return new RequestResult { Success = false, Message = "Kullanıcı sistemde bulunamadı." };
            }

            if (string.IsNullOrEmpty(token))
            {
                await _auditLogger.LogAsync(
                    userId,
                    "FailedAccountDeletion",
                    "Auth/DeleteVerify",
                    null,
                    null,
                    null,
                    "Hesap silme işlemi doğrulama kodu olmadan gerçekleştirilmeye çalışıldı."
                );

                return new RequestResult { Success = false, Message = "Geçersiz doğrulama kodu." };
            }

            if (user.DeleteToken != token)
            {
                await _auditLogger.LogAsync(
                    userId,
                    "FailedAccountDeletion",
                    "Auth/DeleteVerify",
                    null,
                    null,
                    null,
                    "Geçersiz hesap silme doğrulama kodu girildi."
                );

                return new RequestResult { Success = false, Message = "Girilen doğrulama kodu geçerli değil." };
            }

            if (EmailHelper.IsTokenExpired(user.DeleteTokenRequesAt))
            {
                await _auditLogger.LogAsync(
                    userId,
                    "FailedAccountDeletion",
                    "Auth/DeleteVerify",
                    null,
                    null,
                    null,
                    "Hesap silme doğrulama kodunun süresi dolmuş."
                );

                return new RequestResult { Success = false, Message = "Doğrulama kodunun süresi dolmuştur." };
            }

            await _auditLogger.LogAsync(
                null,
                "AccountDeleted",
                "Auth/DeleteVerify",
                null,
                null,
                null,
                $"'{user.Email}' adresine ait kullanıcı hesabı başarıyla silindi."
            );


            await _userRepo.DeleteAsync(user);

            return new RequestResult
            {
                Success = true,
                Message = "Hesabınız başarıyla silindi."
            };
        }

        public async Task<RequestResult> ApproveIpAsync(string token, string email, string password)
        {
            var adminUser = await _userRepo.GetByEmailAsync(email);
            var trustedIp = await _trustedIpRepo.GetByVerificationTokenAsync(token);

            if (string.IsNullOrEmpty(token))
            {
                await _auditLogger.LogAsync(null, "FailedIpApproval", "Auth/ApproveIp", null, null, null, "Token boş.");
                return new RequestResult { Success = false, Message = "Doğrulama kodu eksik. Lütfen tekrar deneyin." };
            }

            if (adminUser == null || !BCrypt.Net.BCrypt.Verify(password, adminUser.PasswordHash))
            {
                return new RequestResult { Success = false, Message = "E-posta adresi veya şifre hatalı. Lütfen kontrol ederek tekrar deneyin." };
            }

            if (trustedIp == null)
            {
                await _auditLogger.LogAsync(null, "FailedIpApproval", "Auth/ApproveIp", adminUser.Id, null, null, "Token eşleşmedi.");
                return new RequestResult { Success = false, Message = "Geçersiz doğrulama kodu. Lütfen bağlantıyı kontrol edin." };
            }

            if (trustedIp.IsApproved)
            {
                await _auditLogger.LogAsync(trustedIp.UserId, "RedundantIpApproval", "Auth/ApproveIp", adminUser.Id, null, null, "IP zaten onaylı.");
                return new RequestResult { Success = true, Message = "IP adresi zaten onaylanmış." };
            }

            if (adminUser.Role?.Name != "Admin")
            {
                await _auditLogger.LogAsync(trustedIp.UserId, "FailedIpApproval", "Auth/ApproveIp", adminUser.Id, null, null, "Yetersiz yetki.");
                return new RequestResult { Success = false, Message = "Bu işlem için yetkiniz bulunmamaktadır." };
            }

            trustedIp.IsApproved = true;
            trustedIp.ApprovedAt = DateTime.UtcNow;
            trustedIp.Token = string.Empty;

            await _trustedIpRepo.UpdateAsync(trustedIp);

            await _auditLogger.LogAsync(
                trustedIp.UserId,
                "IpApproved",
                "Auth/ApproveIp",
                adminUser.Id,
                null,
                null,
                $"IP adresiniz ({trustedIp.IpAddress}), {adminUser.FullName} tarafından onaylandı."
            );

            return new RequestResult
            {
                Success = true,
                Message = "IP adresi başarıyla onaylandı."
            };
        }

        public async Task<ApproveIpResultViewModel> GetIpApprovalDetailsAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ApproveIpResultViewModel
                {
                    Success = false,
                    Message = "Geçersiz bağlantı. Lütfen bağlantının doğruluğunu kontrol edin."
                };
            }

            var trustedIp = await _trustedIpRepo.GetByVerificationTokenAsync(token);

            if (trustedIp == null || trustedIp.User == null)
            {
                return new ApproveIpResultViewModel
                {
                    Success = false,
                    Message = "Bağlantı geçersiz veya süresi dolmuş olabilir. Lütfen yeniden talep edin."
                };
            }

            return new ApproveIpResultViewModel
            {
                Success = true,
                Token = token,
                IpAddress = trustedIp.IpAddress,
                User = trustedIp.User,
                RequestedAt = trustedIp.RequestedAt
            };
        }

        public async Task LogoutAsync(Guid userId, string sessionKey)
        {
            await _userSessionRepo.DeactivateBySessionKeyAsync(userId, sessionKey);

            await _auditLogger.LogAsync(
                userId,
                "Logout",
                "Auth/Logout",
                null,
                sessionKey,
                null,
                "Oturumunuzdan çıkış yaptınız."
            );


        }

    }
}
