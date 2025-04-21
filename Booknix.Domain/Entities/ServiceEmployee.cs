using System;

namespace Booknix.Domain.Entities
{
    public class ServiceEmployee
    {
        public Guid Id { get; set; }

        public Guid ServiceId { get; set; }
        public Guid EmployeeId { get; set; }

        // Navigation
        public Service? Service { get; set; }
        public Worker? Employee { get; set; }
    }
}