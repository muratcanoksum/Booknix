﻿using Booknix.Domain.Entities;
using Booknix.Application.DTOs;
using Booknix.Application.ViewModels;
using Booknix.Domain.Entities.Enums;

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
        Task<IEnumerable<WorkerWithReviewsDto>> GetWorkersWithReviewsAsync(Guid locationId);
        Task<RequestResult> AddWorkerToLocationAsync(WorkerAddDto dto);
        Task<RequestResult> DeleteWorkerAsync(Guid id);
        Task<RequestResult> UpdateWorkerAsync(Guid id, WorkerAddDto dto);
        Task<Worker?> GetWorkerByIdAsync(Guid id);

        // WorkerHours
        Task<List<WorkerWorkingHour>> GetWorkerWorkingHoursAsync(Guid workerId, int year, int month);
        Task<RequestResult> AddWorkerWorkingHourAsync(WorkerWorkingHourDto dto);

        // User
        Task<List<User>> GetAllUsersAsync();
        Task<List<Role>> GetAllRolesAsync();
        Task<RequestResult> UpdateUserAsync(UserUpdateDto model);
        Task<RequestResult> DeleteUserAsync(Guid id, string currentUserId);
        Task<RequestResult> CreateUserAsync(UserCreateDto model);

        // Session
        Task<List<UserSession>> GetActiveSessionsAsync(Guid userId);
        Task<RequestResult> TerminateSessionAsync(Guid userId, string sessionKey);
        Task<RequestResult> TerminateAllSessionsAsync(Guid userId);


        // EmailQueue
        Task<List<EmailQueue>> GetAllEmailQueuesAsync();
        Task<List<EmailQueue>> GetEmailsByStatusAsync(EmailQueueStatus status);
        Task<RequestResult> CancelEmailAsync(Guid id);
        Task<RequestResult> RetryEmailAsync(Guid id);


    }
}
