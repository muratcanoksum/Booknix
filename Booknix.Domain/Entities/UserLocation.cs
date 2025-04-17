using System;


namespace Booknix.Domain.Entities
{

    public enum LocationRole
    {
        LocationAdmin = 0,
        LocationEmployee = 1
    }

    public class UserLocation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LocationId { get; set; }

        public LocationRole RoleInLocation { get; set; }

        public User? User { get; set; }
        public Location? Location { get; set; }
    }
}