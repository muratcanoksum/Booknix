namespace Booknix.Shared.Interfaces
{
    public interface IAppSettings
    {
        string BaseUrl { get; }
        int TokenExpireMinutes { get; }
    }
}
