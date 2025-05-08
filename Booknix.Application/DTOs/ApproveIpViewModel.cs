using Booknix.Domain.Entities;

namespace Booknix.Application.DTOs
{
    public class ApproveIpResultViewModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        // Başarılı ise bu alanlar dolu olur
        public string? Token { get; set; }
        public string? IpAddress { get; set; }
        public User? User { get; set; }
        public DateTime? RequestedAt { get; set; }
    }

}
