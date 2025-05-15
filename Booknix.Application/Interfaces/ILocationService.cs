using Booknix.Application.DTOs;
using Booknix.Application.ViewModels;
using Booknix.Domain.Entities;

namespace Booknix.Application.Interfaces
{
    public interface ILocationService
    {
        Task<Location?> GetLocationByIdAsync(Guid Id);

        Task<ServiceCreateViewModel> GetServicesByLocationAsync(Guid locationId);
        Task<RequestResult> AddServiceToLocationAsync(ServiceCreateDto dto);
        Task<RequestResult> UpdateServiceAsync(ServiceUpdateDto dto);
        Task<RequestResult> DeleteServiceAsync(Guid id);
        Task<ServiceEmployee?> GetServiceEmployeeAsync(Guid serviceId, Guid workerId);
        Task<RequestResult> RemoveWorkerFromServiceAsync(Guid serviceEmployeeId);
        Task<bool> LocationExistsAsync(Guid locationId);
        Task<Service?> GetServiceByIdAsync(Guid id);

        Task<IEnumerable<Worker>> GetAllWorkersAsync(Guid locationId);
        Task<IEnumerable<WorkerWithReviewsDto>> GetWorkersWithReviewsAsync(Guid locationId);
        Task<RequestResult> AddWorkerToLocationAsync(WorkerAddDto dto);
        Task<RequestResult> DeleteWorkerAsync(Guid id, Guid currentUserId);
        Task<RequestResult> UpdateWorkerAsync(Guid id, WorkerAddDto dto);
        Task<Worker?> GetWorkerByIdAsync(Guid id);

        Task<List<WorkerWorkingHour>> GetWorkerWorkingHoursAsync(Guid workerId, int year, int month);
        Task<RequestResult> AddWorkerWorkingHourAsync(WorkerWorkingHourDto dto);



    }
}
