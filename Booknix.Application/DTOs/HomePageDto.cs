namespace Booknix.Application.DTOs;

public class HomePageDto
{
    public List<SectorDto> Sectors { get; set; } = new();
    public List<PopularServiceDto> PopularServices { get; set; } = new();
}

public class SectorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? ImageUrl { get; set; }
    public List<LocationDto> Locations { get; set; } = new();
}

public class LocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string City { get; set; } = "";
}

public class PopularServiceDto
{
    public string Name { get; set; } = "";
    public string LocationName { get; set; } = "";
    public int TotalAppointments { get; set; }
}
