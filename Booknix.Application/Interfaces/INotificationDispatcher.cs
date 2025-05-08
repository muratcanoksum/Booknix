namespace Booknix.Application.Interfaces
{
    public interface INotificationDispatcher
    {
        Task PushAsync(string key, object? payload = null);
    }
}
