using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booknix.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public Guid RoleId { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? TokenGeneratedAt { get; set; }
        public DateTime? EmailVerifiedAt { get; set; } // Opsiyonel: ne zaman doğrulandı
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetRequestedAt { get; set; }



        // Navigation
        public Role? Role { get; set; }
        public ICollection<UserLocation> UserLocations { get; set; } = new List<UserLocation>();
        public ICollection<Service> Services { get; set; } = new List<Service>();

    }
}