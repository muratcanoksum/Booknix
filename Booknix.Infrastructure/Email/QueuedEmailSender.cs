using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Interfaces;

namespace Booknix.Infrastructure.Email
{
    public class QueuedEmailSender : IEmailSender
    {
        private readonly IEmailQueueRepository _queueRepo;
        private readonly IEmailQueueNotifier _emailQueueNotifier;

        public QueuedEmailSender(IEmailQueueRepository queueRepo, IEmailQueueNotifier emailQueueNotifier)
        {
            _queueRepo = queueRepo;
            _emailQueueNotifier = emailQueueNotifier;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string from)
        {
            var queueItem = new EmailQueue
            {
                Id = Guid.NewGuid(),
                To = to,
                Subject = subject,
                Body = htmlBody,
                From = from,
                Status = EmailQueueStatus.Pending,
                TryCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _queueRepo.AddAsync(queueItem);
            await _emailQueueNotifier.NotifyCreatedAsync(queueItem);
        }
    }
}
