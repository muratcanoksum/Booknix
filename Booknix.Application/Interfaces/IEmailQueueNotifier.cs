using Booknix.Domain.Entities;

namespace Booknix.Application.Interfaces
{
    public interface IEmailQueueNotifier
    {
        Task NotifyStatusChangedAsync(EmailQueue email, string? oldStatus = null);
        Task NotifyCreatedAsync(EmailQueue email);
    }
}
