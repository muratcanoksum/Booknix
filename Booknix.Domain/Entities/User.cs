﻿using System;
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
        public string? MailChangeVerifyToken { get; set; }
        public DateTime? MailChangeRequestedAt { get; set; }
        public string? PendingEmail { get; set; } // (Opsiyonel ama önerilir)
        public DateTime? EmailChangedAt { get; set; }
        public string? PreviousEmail { get; set; }
        public DateTime? DeleteTokenRequesAt { get; set; }
        public string? DeleteToken { get; set; }



        // Navigation
        public Role? Role { get; set; }
        public UserProfile? Profile { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
        public ICollection<TrustedIp> TrustedIps { get; set; } = new List<TrustedIp>();




    }
}