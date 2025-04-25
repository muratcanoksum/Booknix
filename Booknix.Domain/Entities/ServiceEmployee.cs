using System;

namespace Booknix.Domain.Entities
{
    public class ServiceEmployee
    {
        public Guid Id { get; set; }

        public Guid ServiceId { get; set; }
        public Guid WorkerId { get; set; } // eski adý EmployeeId

        public Service? Service { get; set; }
        public Worker? Worker { get; set; }
    }

}