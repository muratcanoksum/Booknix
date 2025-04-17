using System;

namespace Booknix.Domain.Entities
{
    public class UserProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? ProfileImagePath { get; set; }

        // Navigation
        public User? User { get; set; }
    }
}
