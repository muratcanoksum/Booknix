using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces;

public interface IPublicService
{
    Task<HomePageDto> GetHomePageDataAsync();
    List<SearchResultDto> SearchLocationsAndServices(string query);
    SectorLocationsDto? GetLocationsBySectorSlug(string slug);
    LocationDetailsDto? GetLocationDetails(string slug);
    ServiceDetailsDto? GetServiceDetails(string locationSlug, Guid serviceId);

}
