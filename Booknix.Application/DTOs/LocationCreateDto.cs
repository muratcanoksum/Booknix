using Microsoft.AspNetCore.Mvc.Rendering;

namespace Booknix.Application.DTOs;
public class LocationCreateDto
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public Guid SectorId { get; set; }
    public IEnumerable<SelectListItem> Sectors { get; set; } = new List<SelectListItem>();
}
