using Booknix.Domain.Entities.Enums;

namespace Booknix.Application.DTOs
{
    public class WorkerAddDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public Guid LocationId { get; set; }

        public LocationRole RoleInLocation { get; set; }  // enum alanı
    }

}
