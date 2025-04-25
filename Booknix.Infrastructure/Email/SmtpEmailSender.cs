using DotNetEnv;
using MailKit.Net.Smtp;
using MimeKit;
using Booknix.Application.Interfaces;

namespace Booknix.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string to, string subject, string htmlBody, string from)
        {
            // Ortamı oku
            var environment = Env.GetString("ASPNETCORE_ENVIRONMENT") ?? "Production";

            string finalRecipient = to;

            if (environment == "Development")
            {
                subject += $" [DEV REDIRECT TO: {to}]";
                finalRecipient = "temp@booknix.ismailparlak.com";
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(from, Env.GetString("EMAIL_HOST_USER")));
            message.To.Add(MailboxAddress.Parse(finalRecipient));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
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
