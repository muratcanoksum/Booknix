namespace Booknix.Application.DTOs
{
    public class ServiceCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public Guid LocationId { get; set; }
    }
}
