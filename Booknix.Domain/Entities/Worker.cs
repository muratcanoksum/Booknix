using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Entities;

public class Worker
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public LocationRole RoleInLocation { get; set; }
}
