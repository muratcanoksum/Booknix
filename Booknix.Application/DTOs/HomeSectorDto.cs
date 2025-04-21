namespace Booknix.Application.DTOs
{
    public class HomeSectorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int LocationCount { get; set; }
    }
}
