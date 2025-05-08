using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;

public class EmailQueueNotifier : IEmailQueueNotifier
{
    private readonly INotificationDispatcher _dispatcher;

    public EmailQueueNotifier(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task NotifyStatusChangedAsync(EmailQueue email, string? oldStatus = null)
    {
        if (!string.IsNullOrEmpty(oldStatus) && oldStatus != email.Status.ToString())
        {
            // Önceki tab'dan silinmeli
            await _dispatcher.PushAsync("emailQueueUpdated", oldStatus);
        }

        // Yeni tab'a eklenmeli
        await _dispatcher.PushAsync("emailQueueUpdated", email.Status.ToString());
    }


    public async Task NotifyCreatedAsync(EmailQueue email)
    {
        await _dispatcher.PushAsync("emailQueueUpdated", "Pending"); // Yeni eklendiği için Pending'e düşer
    }
}
