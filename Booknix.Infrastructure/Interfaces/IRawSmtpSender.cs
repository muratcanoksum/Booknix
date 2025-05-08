namespace Booknix.Infrastructure.Interfaces
{
    public interface IRawSmtpSender
    {
        Task SendRawAsync(string to, string subject, string htmlBody, string from);
    }
}
