using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Booknix.Application.Services;

public class HomeService : IHomeService
{
    private readonly ISectorRepository _sectorRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly IMemoryCache _cache;

    public HomeService(
        ISectorRepository sectorRepo,
        IAppointmentRepository appointmentRepo,
        IServiceRepository serviceRepo,
        IMemoryCache cache)
    {
        _sectorRepo = sectorRepo;
        _appointmentRepo = appointmentRepo;
        _serviceRepo = serviceRepo;
        _cache = cache;
    }

    public async Task<HomePageDto> GetHomePageDataAsync()
    {
        var dto = new HomePageDto();

        var sectors = await _sectorRepo.GetAllWithLocationsAndMediaAsync();

        dto.Sectors = sectors.Select(s => new SectorDto
        {
            Id = s.Id,
            Name = s.Name,
            Slug = s.Slug,
            ImageUrl = s.MediaFiles.FirstOrDefault()?.FilePath ?? "/images/default/sector.png",
            Locations = s.Locations.Select(l => new LocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Slug = l.Name.ToLower().Replace(" ", "-"),
                City = l.Address.Split(" ").FirstOrDefault() ?? "-"
            }).ToList()
        }).ToList();

        // Popüler hizmetler - cache'li
        if (!_cache.TryGetValue("popular_services", out List<PopularServiceDto> popular))
        {
            var appointments = await _appointmentRepo.GetAllAsync();
            var services = await _serviceRepo.GetAllAsync();

            popular = services.Select(s => new PopularServiceDto
            {
                Name = s.Name,
                LocationName = s.Location?.Name ?? "-",
                TotalAppointments = appointments.Count(a => a.ServiceId == s.Id)
            })
            .OrderByDescending(x => x.TotalAppointments)
            .Take(6)
            .ToList();

            _cache.Set("popular_services", popular, TimeSpan.FromMinutes(10));
        }

        dto.PopularServices = popular;

        return dto;
    }
}
