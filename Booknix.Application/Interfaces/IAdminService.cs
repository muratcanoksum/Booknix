using Booknix.Domain.Entities;
using Booknix.Application.DTOs;
using Booknix.Application.ViewModels;

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
        Task<bool> LocationExistsAsync(Guid locationId);


        // Services
        Task<ServiceCreateViewModel> GetServicesByLocationAsync(Guid locationId);
        Task<RequestResult> AddServiceToLocationAsync(ServiceCreateDto dto);
        Task<RequestResult> DeleteServiceAsync(Guid id);
        Task<RequestResult> UpdateServiceAsync(ServiceUpdateDto dto);
        Task<Service?> GetServiceByIdAsync(Guid id);
        Task<ServiceEmployee?> GetServiceEmployeeAsync(Guid serviceId, Guid workerId);
        Task<RequestResult> RemoveWorkerFromServiceAsync(Guid serviceEmployeeId);

        // Workers
        Task<IEnumerable<Worker>> GetAllWorkersAsync(Guid locationId);
        Task<RequestResult> AddWorkerToLocationAsync(WorkerAddDto dto);
        Task<RequestResult> DeleteWorkerAsync(Guid id);
        Task<RequestResult> UpdateWorkerAsync(Guid id, WorkerAddDto dto);
        Task<Worker?> GetWorkerByIdAsync(Guid id);
    }
}
