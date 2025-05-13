namespace Booknix.Application.Interfaces
{
    public interface INotificationDispatcher
    {
        Task PushAsync(string key, object? payload = null);
        Task PushToUserAsync(Guid userId, string key, object? payload = null);
    }
}
