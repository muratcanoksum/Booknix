using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;
using Booknix.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Booknix.Application.Services;

public class PublicService(
    ISectorRepository sectorRepo,
    IAppointmentRepository appointmentRepo,
    IAppointmentSlotRepository appointmentSlotRepo,
    IServiceRepository serviceRepo,
    IMemoryCache cache,
    ILocationRepository locationRepo,
    IWorkerWorkingHourRepository workingHourRepo,
    IWorkerRepository workerRepo,
    IUserRepository userRepo,
    IReviewRepository reviewRepo
        ) : IPublicService
{
    private readonly ISectorRepository _sectorRepo = sectorRepo;
    private readonly IUserRepository _userRepo = userRepo;
    private readonly IAppointmentSlotRepository _appointmentSlotRepo = appointmentSlotRepo;
    private readonly IAppointmentRepository _appointmentRepo = appointmentRepo;
    private readonly IServiceRepository _serviceRepo = serviceRepo;
    private readonly IMemoryCache _cache = cache;
    private readonly ILocationRepository _locationRepo = locationRepo;
    private readonly IWorkerWorkingHourRepository _workingHourRepo = workingHourRepo;
    private readonly IWorkerRepository _workerRepo = workerRepo;
    private readonly IReviewRepository _reviewRepo = reviewRepo;


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

        var workers = service.ServiceEmployees.Select(se => new WorkerMiniDto
        {
            FullName = se.Worker!.User.FullName,
            Id = se.Worker.Id,
            ReviewCount = 0,
            AverageRating = 0
        }).ToList();

        // Get review statistics for each worker
        foreach (var worker in workers)
        {
            var stats = _reviewRepo.GetWorkerReviewStatsAsync(worker.Id).GetAwaiter().GetResult();
            worker.ReviewCount = stats.Count;
            worker.AverageRating = stats.AverageRating;
        }

        return new ServiceDetailsDto
        {
            ServiceName = service.Name,
            ServiceId = serviceId.ToString(),
            Description = service.Description,
            Price = service.Price,
            Duration = service.Duration,
            LocationSlug = service.Location.Slug,
            LocationName = service.Location.Name,
            Workers = workers
        };
    }

    public async Task<AppointmentSlotPageDto> GetAppointmentSlotPageData(Guid workerId, Guid serviceId, DateTime startDate, DateTime endDate, TimeSpan currentTime)
    {
        var service = await _serviceRepo.GetByIdAsync(serviceId);
        if (service == null) return new AppointmentSlotPageDto();

        var worker = await _workerRepo.GetByIdAsync(workerId);
        if (worker == null) return new AppointmentSlotPageDto();

        var location = service.Location;
        if (location == null) return new AppointmentSlotPageDto();

        var result = new AppointmentSlotPageDto
        {
            LocationName = location.Name,
            ServiceName = service.Name,
            Price = service.Price,
            Duration = service.Duration,
            WorkerName = worker.User?.FullName ?? "-"
        };

        var serviceGap = service.ServiceGap;
        var workingDays = _workingHourRepo.GetValidWorkingDays(workerId, startDate, endDate);

        var takenAppointments = _appointmentRepo
            .GetByWorkerBetweenDates(workerId, startDate, endDate)
            .Select(a => new
            {
                Start = a.AppointmentSlot!.StartTime,
                End = a.AppointmentSlot!.EndTime,
                Status = a.Status // Artık gerçek appointment status
            }).ToList();

        foreach (var day in workingDays)
        {
            var baseDate = day.Date.Date;
            var slotTime = day.StartTime!.Value;
            var endTime = day.EndTime!.Value;
            var lunchStart = baseDate.Add(location.LunchBreakStart);
            var lunchEnd = baseDate.Add(location.LunchBreakEnd);

            while (true)
            {
                var potentialStart = baseDate.Add(slotTime);
                var potentialEnd = potentialStart.Add(service.Duration);

                if (potentialEnd - serviceGap > baseDate.Add(endTime))
                    break;

                // Bugün ve geçmiş saat mi?
                if (baseDate == startDate.Date && slotTime <= currentTime)
                {
                    slotTime += service.Duration + serviceGap;
                    continue;
                }

                // Öğle arasına denk geliyor mu?
                if (potentialStart < lunchEnd && potentialEnd > lunchStart)
                {
                    slotTime = location.LunchBreakEnd;
                    continue;
                }

                // Randevu çakışma kontrolü (tam bitiş zamanı hariç)
                var existing = takenAppointments.FirstOrDefault(r =>
                    r.Start < potentialEnd &&
                    r.End > potentialStart &&
                    r.End != potentialStart);

                AppointmentStatus? status = existing?.Status;

                result.Slots.Add(new AppointmentSlotDto
                {
                    Date = baseDate,
                    Time = slotTime,
                    IsAvailable =
                        status == null ||
                        status == AppointmentStatus.Cancelled ||
                        status == AppointmentStatus.NoShow,
                    Status = status
                });


                slotTime += service.Duration + serviceGap;
            }
        }

        result.Slots = result.Slots
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Time)
            .ToList();

        return result;
    }

    public async Task<RequestResult> CreateAppointmentAsync(Guid userId, CreateAppointmentDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            return new RequestResult(false, "Kullanıcı bulunamadı");

        var worker = await _workerRepo.GetByIdAsync(dto.WorkerId);
        if (worker == null)
            return new RequestResult(false, "Çalışan bulunamadı");

        var service = await _serviceRepo.GetByIdAsync(dto.ServiceId);
        if (service == null)
            return new RequestResult(false, "Hizmet bulunamadı");

        var location = await _locationRepo.GetByIdAsync(service.LocationId);
        if (location == null)
            return new RequestResult(false, "Lokasyon bulunamadı");

        var serviceSlots = await GetAppointmentSlotPageData(
            dto.WorkerId,
            dto.ServiceId,
            dto.Date,
            dto.Date.AddDays(1),
            TimeSpan.Zero
        );

        var slot = serviceSlots.Slots.FirstOrDefault(x => x.Date == dto.Date && x.Time == dto.Time);
        if (slot == null)
            return new RequestResult(false, "Randevu bulunamadı");

        if (!slot.IsAvailable)
            return new RequestResult(false, "Randevu dolu");

        var startTime = DateTime.SpecifyKind(dto.Date.Date.Add(dto.Time), DateTimeKind.Utc);
        var endTime = startTime.Add(service.Duration);


        var appointmentSlot = new AppointmentSlot
        {
            Id = Guid.NewGuid(),
            StartTime = startTime,
            EndTime = endTime,
            ServiceId = dto.ServiceId,
            LocationId = service.LocationId,
            AssignerWorkerId = dto.WorkerId,
            Status = SlotStatus.Booked
        };

        try
        {
            await _appointmentSlotRepo.AddAsync(appointmentSlot);
        }
        catch (Exception ex)
        {
            return new RequestResult(false, $"Slot oluşturulamadı. Hata: {ex.Message}");
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ServiceId = dto.ServiceId,
            AppointmentSlotId = appointmentSlot.Id,
            Notes = dto.Notes,
            Status = AppointmentStatus.Pending
        };

        try
        {
            await _appointmentRepo.AddAsync(appointment);
        }
        catch (Exception ex)
        {
            await _appointmentSlotRepo.DeleteAsync(appointmentSlot.Id);
            return new RequestResult(false, $"Randevu oluşturulamadı, slot geri alındı. Hata: {ex.Message}");
        }

        return new RequestResult(true, "Randevu başarıyla oluşturuldu");
    }







}
