using Booknix.Application.DTOs;
using Booknix.Domain.Entities;
using Booknix.Application.ViewModels;

namespace Booknix.Application.Interfaces
{
    public interface ILocationAdminService
    {
        // Locations
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<RequestResult> AddLocationAsync(string name, string address, string phoneNumber, Guid sectorId);
        Task<RequestResult> DeleteLocationAsync(Guid Id);
        Task<Location?> GetLocationByIdAsync(Guid Id);
        Task<RequestResult> UpdateLocationAsync(Guid id, string name, string address, string phoneNumber, Guid sectorId);
        
        // Services
        Task<List<Service>> GetServicesByLocationAsync(Guid locationId);
        Task<ServiceCreateViewModel> GetServiceCreateViewModelAsync(Guid locationId);
        Task<Service?> GetServiceByIdAsync(Guid id);
        Task<RequestResult> AddServiceAsync(ServiceCreateDto model);
        Task<RequestResult> UpdateServiceAsync(ServiceUpdateDto model);
        Task<RequestResult> DeleteServiceAsync(Guid id);
        Task<RequestResult> RemoveWorkerFromServiceAsync(Guid serviceId, Guid workerId);

        // Workers
        Task<IEnumerable<Worker>> GetWorkersByLocationAsync(Guid locationId);
        Task<RequestResult> AddWorkerAsync(WorkerAddDto dto);
        Task<RequestResult> UpdateWorkerAsync(WorkerUpdateDto dto);
        Task<RequestResult> DeleteWorkerAsync(Guid id);
        Task<Worker?> GetWorkerByIdAsync(Guid id);
    }
}
