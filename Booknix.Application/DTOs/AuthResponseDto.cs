using Booknix.Domain.Entities.Enums;

namespace Booknix.Application.DTOs
{
    public class AuthResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public Guid? LocationId { get; set; } = null!;
        public LocationRole? LocationRole { get; set; } = null!;
    }
}
