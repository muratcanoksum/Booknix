using System;

namespace Booknix.Application.DTOs
{
    public class UserUpdateDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Guid RoleId { get; set; }
    }
} 