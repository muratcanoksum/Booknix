﻿using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces;

public interface IPublicService
{
    Task<HomePageDto> GetHomePageDataAsync();
    List<SearchResultDto> SearchLocationsAndServices(string query);
    SectorLocationsDto? GetLocationsBySectorSlug(string slug);
    LocationDetailsDto? GetLocationDetails(string slug);
    ServiceDetailsDto? GetServiceDetails(string locationSlug, Guid serviceId);
    Task<AppointmentSlotPageDto> GetAppointmentSlotPageData(Guid workerId, Guid serviceId, DateTime startDate, DateTime endDate, TimeSpan currentTime);
    Task<RequestResult> CreateAppointmentAsync(Guid userId, CreateAppointmentDto dto);

}
