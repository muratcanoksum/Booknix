using DotNetEnv;
using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;

namespace Booknix.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Booknix Doğrulama", Env.GetString("EMAIL_HOST_USER")));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(Env.GetString("EMAIL_HOST"), int.Parse(Env.GetString("EMAIL_PORT")), bool.Parse(Env.GetString("EMAIL_USE_SSL")));
            await client.AuthenticateAsync(Env.GetString("EMAIL_HOST_USER"), Env.GetString("EMAIL_HOST_PASSWORD"));
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
