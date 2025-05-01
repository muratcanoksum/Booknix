using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces;

public interface IHomeService
{
    Task<HomePageDto> GetHomePageDataAsync();
    List<SearchResultDto> SearchLocationsAndServices(string query);
}
