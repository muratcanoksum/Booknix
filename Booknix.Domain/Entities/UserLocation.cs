using System;
using Booknix.Domain.Entities.Enums;


namespace Booknix.Domain.Entities
{
    

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