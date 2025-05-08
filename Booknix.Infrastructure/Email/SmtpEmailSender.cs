using DotNetEnv;
using MailKit.Net.Smtp;
using MimeKit;
using Booknix.Application.Interfaces;
using Booknix.Infrastructure.Interfaces;


namespace Booknix.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender, IRawSmtpSender
    {
        public Task SendEmailAsync(string to, string subject, string htmlBody, string from)
        {
            // Kuyruk için çağrılan IEmailSender tarafı (zaten fire-and-forget içine alınmıştı)
            return Task.Run(() => SendRawAsync(to, subject, htmlBody, from));
        }

        public async Task SendRawAsync(string to, string subject, string htmlBody, string from)
        {
            var environment = Env.GetString("ASPNETCORE_ENVIRONMENT") ?? "Production";

            string finalRecipient = to;
            if (environment == "Development")
            {
                subject += $" [DEV REDIRECT TO: {to}]";
                finalRecipient = "temp@booknix.muratcanoksum.com";
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, Env.GetString("EMAIL_HOST_USER")));
            message.To.Add(MailboxAddress.Parse(finalRecipient));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient
            {
                Timeout = 10000 // 10 saniye
            };

            await client.ConnectAsync(
                Env.GetString("EMAIL_HOST"),
                int.Parse(Env.GetString("EMAIL_PORT")),
                bool.Parse(Env.GetString("EMAIL_USE_SSL"))
            );

            await client.AuthenticateAsync(
                Env.GetString("EMAIL_HOST_USER"),
                Env.GetString("EMAIL_HOST_PASSWORD")
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
