namespace Booknix.Domain.Entities;

public class TrustedIp
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string IpAddress { get; set; } = null!;
    public bool IsApproved { get; set; } = false;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
