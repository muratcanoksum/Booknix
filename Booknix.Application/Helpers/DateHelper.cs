namespace Booknix.Application.Helpers
{
    public static class DateHelper
    {
        public static DateTime ToUtc(DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
        }

        public static DateTime? ToLocal(DateTime? dt)
        {
            if (dt == null)
                return null;

            return dt.Value.Kind switch
            {
                DateTimeKind.Local => dt,
                DateTimeKind.Utc => dt.Value.ToLocalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(dt.Value, DateTimeKind.Local),
                _ => DateTime.SpecifyKind(dt.Value, DateTimeKind.Local)
            };
        }


    }
}
