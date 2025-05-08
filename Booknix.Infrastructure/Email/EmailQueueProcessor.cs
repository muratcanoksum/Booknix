using Booknix.Application.Interfaces;
using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Interfaces;
using Booknix.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Booknix.Infrastructure.Email
{
    public class EmailQueueProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public EmailQueueProcessor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Email Queue Processor started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var queueRepo = scope.ServiceProvider.GetRequiredService<IEmailQueueRepository>();
                var smtpSender = scope.ServiceProvider.GetRequiredService<IRawSmtpSender>();

                var pendingEmails = await queueRepo.GetPendingAsync(limit: 10);

                foreach (var email in pendingEmails)
                {
                    try
                    {
                        await smtpSender.SendRawAsync(email.To, email.Subject, email.Body, email.From);
                        email.Status = EmailQueueStatus.Sent;
                        email.SentAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        email.Status = EmailQueueStatus.Failed;
                        email.ErrorMessage = ex.Message;
                        email.TryCount++;
                    }

                    await queueRepo.UpdateAsync(email);
                }

                await Task.Delay(10000, stoppingToken); // 5 saniyede bir döngü
            }
        }
    }
}
