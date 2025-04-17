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
        public UserProfile? Profile { get; set; }
        public ICollection<UserLocation> UserLocations { get; set; } = new List<UserLocation>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<ServiceEmployee> ServiceEmployees { get; set; } = new List<ServiceEmployee>();
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        
        public ICollection<TrustedIp> TrustedIps { get; set; } = new List<TrustedIp>();




    }
}