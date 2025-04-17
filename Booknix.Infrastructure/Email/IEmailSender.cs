namespace Booknix.Infrastructure.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string htmlBody,string from);
    }
}
