
namespace Booknix.Application.DTOs
{
    public class CreateAppointmentDto
    {
        public Guid ServiceId { get; set; }
        public Guid WorkerId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string? Notes { get; set; }
        public string Slug { get; set; } = string.Empty; // yönlendirme için gerekli
    }
}
