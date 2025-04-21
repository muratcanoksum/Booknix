using Booknix.Domain.Entities;
using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface IAdminService
    {
        // Sectors

        Task<IEnumerable<Sector>> GetAllSectorsAsync();
        Task<RequestResult> AddSectorAsync(string Name);
        Task<RequestResult> DeleteSectorAsync(Guid Id);
        Task<Sector?> GetSectorByIdAsync(Guid Id);
        Task<RequestResult> UpdateSectorAsync(Guid Id, string Name);

        // Locations
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<RequestResult> AddLocationAsync(string name, string address, string phoneNumber, Guid sectorId);
        Task<RequestResult> DeleteLocationAsync(Guid Id);
        Task<Location?> GetLocationByIdAsync(Guid Id);
        Task<RequestResult> UpdateLocationAsync(Guid id, string name, string address, string phoneNumber, Guid sectorId);

        //
    }
}
