using Booknix.Domain.Entities.Enums;

namespace Booknix.Application.DTOs
{
    public class WorkerUpdateDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public LocationRole RoleInLocation { get; set; }
        public Guid LocationId { get; set; }
    }
} 