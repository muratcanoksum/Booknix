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
        Task<ServiceCreateViewModel> GetServicesByLocationAsync(Guid locationId);
        Task<RequestResult> AddServiceToLocationAsync(ServiceCreateDto dto);
        Task<Service?> GetServiceByIdAsync(Guid id);
        Task<RequestResult> UpdateServiceAsync(ServiceUpdateDto dto);
        Task<RequestResult> DeleteServiceAsync(Guid id);
        Task<ServiceEmployee?> GetServiceEmployeeAsync(Guid serviceId, Guid workerId);
        Task<RequestResult> RemoveWorkerFromServiceAsync(Guid serviceEmployeeId);
        Task<bool> LocationExistsAsync(Guid locationId);
        
        // Workers
        Task<IEnumerable<Worker>> GetAllWorkersAsync(Guid locationId);
        Task<RequestResult> AddWorkerToLocationAsync(WorkerAddDto dto);
        Task<RequestResult> DeleteWorkerAsync(Guid id, Guid currentUserId);
        Task<RequestResult> UpdateWorkerAsync(Guid id, WorkerAddDto dto);
        Task<Worker?> GetWorkerByIdAsync(Guid id);
        Task<RequestResult> UpdateWorkerEmailAsync(Guid workerId, string newEmail);

        // WorkerHours
        Task<List<WorkerWorkingHour>> GetWorkerWorkingHoursAsync(Guid workerId, int year, int month);
        Task<RequestResult> AddWorkerWorkingHourAsync(WorkerWorkingHourDto dto);
    }
}
