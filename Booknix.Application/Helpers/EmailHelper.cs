namespace Booknix.Application.Helpers
{
    public static class EmailHelper
    {
        private static int _tokenExpireMinutes;

        public static void Configure(int minutes)
        {
            _tokenExpireMinutes = minutes;
        }

        public static int TokenExpireMinutes => _tokenExpireMinutes;

        // Yeni overload: opsiyonel dışarıdan dakika alır
        public static bool IsTokenExpired(DateTime? generatedAtUtc, int? customMinutes = null)
        {
            if (generatedAtUtc == null)
                return true;

            var expireMinutes = customMinutes ?? _tokenExpireMinutes;

            return generatedAtUtc.Value.AddMinutes(expireMinutes) < DateTime.UtcNow;
        }
    }
}
