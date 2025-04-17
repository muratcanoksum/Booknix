namespace Booknix.Application.Helpers
{
    public static class EmailVerificationHelper
    {
        // Email doğrulama token süresi (dakika cinsinden)
        public const int TokenExpireMinutes = 15;

        // Token süresi dolmuş mu kontrolü
        public static bool IsTokenExpired(DateTime? generatedAtUtc)
        {
            if (generatedAtUtc == null)
                return true;

            return generatedAtUtc.Value.AddMinutes(TokenExpireMinutes) < DateTime.UtcNow;
        }
    }
}
