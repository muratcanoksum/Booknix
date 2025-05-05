using System;

namespace Booknix.Application.DTOs
{
    public class UserCreateDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public Guid RoleId { get; set; }
    }
} 