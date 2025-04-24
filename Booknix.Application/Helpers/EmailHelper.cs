namespace Booknix.Application.Helpers
{
    public static class EmailHelper
    {
        private static int _tokenExpireMinutes;
        private static string? _baseUrl;

        public static void Configure(int minutes, string baseUrl)
        {
            _tokenExpireMinutes = minutes;
            _baseUrl = baseUrl;
        }

        public static int TokenExpireMinutes => _tokenExpireMinutes;

        public static string BaseUrl => _baseUrl!;

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
