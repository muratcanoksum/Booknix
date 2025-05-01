using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Booknix.Application.Services;

public class PublicService : IPublicService
{
    private readonly ISectorRepository _sectorRepo;
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly IMemoryCache _cache;
    private readonly ILocationRepository _locationRepo;
    private readonly IWorkerWorkingHourRepository _workingHourRepo;
    private readonly IWorkerRepository _workerRepo;

    public PublicService(
        ISectorRepository sectorRepo,
        IAppointmentRepository appointmentRepo,
        IServiceRepository serviceRepo,
        IMemoryCache cache,
        ILocationRepository locationRepo,
        IWorkerWorkingHourRepository workingHourRepo,
        IWorkerRepository workerRepo
        )
    {
        _sectorRepo = sectorRepo;
        _appointmentRepo = appointmentRepo;
        _serviceRepo = serviceRepo;
        _cache = cache;
        _locationRepo = locationRepo;
        _workingHourRepo = workingHourRepo;
        _workerRepo = workerRepo;

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
                City = l.Address.Split(" ").FirstOrDefault() ?? "-",
                ServicesCount = l.Services.Count // Eklenen hizmet sayısı
            }).ToList(),
            LocationsCount = s.Locations.Count // Eklenen lokasyon sayısı 
        }).ToList();

        // Popüler hizmetler - cache'li
        if (!_cache.TryGetValue("popular_services", out List<PopularServiceDto>? popular))
        {
            var appointments = await _appointmentRepo.GetAllAsync();
            var services = await _serviceRepo.GetAllAsync();

            popular = services.Select(s => new PopularServiceDto
            {
                Name = s.Name,
                LocationName = s.Location?.Name ?? "-",
                TotalAppointments = appointments.Count(a => a.ServiceId == s.Id),
            })
            .OrderByDescending(x => x.TotalAppointments)
            .Take(6)
            .ToList();

            _cache.Set("popular_services", popular, TimeSpan.FromMinutes(10));
        }

        dto.PopularServices = popular!;

        return dto;
    }

    public List<SearchResultDto> SearchLocationsAndServices(string query)
    {
        var locationEntities = _locationRepo.Search(query);
        var serviceEntities = _serviceRepo.Search(query);

        var locationResults = locationEntities.Select(x => new SearchResultDto
        {
            Name = x.Name,
            Url = $"/location/{x.Slug}",
            CategoryName = x.Sector?.Name
        });

        var serviceResults = serviceEntities.Select(x => new SearchResultDto
        {
            Name = x.Name,
            Url = $"/location/{x.Location?.Slug}/{x.Id}",
            LocationName = x.Location?.Name
        });

        return locationResults
            .Concat(serviceResults)
            .OrderBy(x => x.Name)
            .Take(20)
            .ToList();
    }

    public SectorLocationsDto? GetLocationsBySectorSlug(string slug)
    {
        var sector = _sectorRepo.GetBySlugWithLocations(slug); // bunu repository'de yazacağız

        if (sector == null) return null;

        return new SectorLocationsDto
        {
            SectorName = sector.Name,
            Locations = sector.Locations.Select(l => new LocationCardDto
            {
                Name = l.Name,
                Address = l.Address,
                Slug = l.Slug,
                ImageUrl = l.MediaFiles.FirstOrDefault()?.FilePath ?? "/images/default/location.png"
            }).ToList()
        };
    }

    public LocationDetailsDto? GetLocationDetails(string slug)
    {
        var location = _locationRepo.GetBySlugWithServices(slug);
        if (location == null) return null;

        return new LocationDetailsDto
        {
            Name = location.Name,
            Address = location.Address,
            Slug = location.Slug,
            ImageUrl = location.MediaFiles.FirstOrDefault()?.FilePath ?? "/images/default/location.png",
            Services = location.Services.Select(s => new ServiceCardDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Duration = s.Duration,
                Description = s.Description
            }).ToList()
        };
    }

    public ServiceDetailsDto? GetServiceDetails(string locationSlug, Guid serviceId)
    {
        var service = _serviceRepo.GetByIdWithLocationAndEmployees(serviceId);
        if (service == null || service.Location?.Slug != locationSlug) return null;

        return new ServiceDetailsDto
        {
            ServiceName = service.Name,
            ServiceId = serviceId.ToString(),
            Description = service.Description,
            Price = service.Price,
            Duration = service.Duration,
            LocationSlug = service.Location.Slug,
            LocationName = service.Location.Name,
            Workers = service.ServiceEmployees.Select(se => new WorkerMiniDto
            {
                FullName = se.Worker!.User.FullName,
                Id = se.Worker.Id
            }).ToList()
        };
    }

    public async Task<AppointmentSlotPageDto> GetAppointmentSlotPageData(Guid workerId, Guid serviceId, DateTime startDate, DateTime endDate, TimeSpan currentTime)
    {
        var service = await _serviceRepo.GetByIdAsync(serviceId);
        if (service == null) return new AppointmentSlotPageDto();

        var worker = await _workerRepo.GetByIdAsync(workerId);
        if (worker == null) return new AppointmentSlotPageDto();

        var result = new AppointmentSlotPageDto
        {
            LocationName = service.Location?.Name ?? "-",
            ServiceName = service.Name,
            Price = service.Price,
            Duration = service.Duration,
            WorkerName = worker.User?.FullName ?? "-"
        };

        var workingDays = _workingHourRepo.GetValidWorkingDays(workerId, startDate, endDate);
        var takenSlots = _appointmentRepo.GetByWorkerBetweenDates(workerId, startDate, endDate)
            .Select(a => new
            {
                Date = a.AppointmentSlot!.StartTime.Date,
                Time = a.AppointmentSlot!.StartTime.TimeOfDay
            }).ToList();

        foreach (var day in workingDays)
        {
            var slotTime = day.StartTime!.Value;
            var isToday = day.Date.Date == startDate.Date;

            while (slotTime <= day.EndTime - service.Duration)
            {
                if (isToday && slotTime <= currentTime)
                {
                    slotTime += service.Duration;
                    continue;
                }

                bool isTaken = takenSlots.Any(x => x.Date == day.Date && x.Time == slotTime);
                if (!isTaken)
                {
                    result.Slots.Add(new AppointmentSlotDto
                    {
                        Date = day.Date,
                        Time = slotTime,
                        IsAvailable = true
                    });
                }

                slotTime += service.Duration;
            }
        }

        result.Slots = result.Slots.OrderBy(x => x.Date).ThenBy(x => x.Time).ToList();
        return result;
    }




}
