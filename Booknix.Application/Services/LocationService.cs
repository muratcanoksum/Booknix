﻿using Booknix.Application.DTOs;
using Booknix.Application.Helpers;
using Booknix.Application.Interfaces;
using Booknix.Application.ViewModels;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Shared.Helpers;
using Microsoft.AspNetCore.Http;

namespace Booknix.Application.Services
{
    public class LocationService(
        ILocationRepository locationRepo,
        IServiceRepository serviceRepo,
        IWorkerRepository workerRepo,
        IServiceEmployeeRepository serviceEmployeeRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IEmailSender emailSender,
        IWorkerWorkingHourRepository workerWorkingHourRepo,
        IHttpContextAccessor httpContextAccesor,
        IReviewRepository reviewRepo
        ) : ILocationService
    {
        private readonly ILocationRepository _locationRepo = locationRepo;
        private readonly IServiceRepository _serviceRepo = serviceRepo;
        private readonly IWorkerRepository _workerRepo = workerRepo;
        private readonly IServiceEmployeeRepository _serviceEmployeeRepo = serviceEmployeeRepo;
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IWorkerWorkingHourRepository _workerWorkingHourRepo = workerWorkingHourRepo;
        private readonly IHttpContextAccessor _httpContextAccesor = httpContextAccesor;
        private readonly IReviewRepository _reviewRepo = reviewRepo;

        private bool IsInLocationAdminPanel =>
    _httpContextAccesor.HttpContext?.Request.Path.StartsWithSegments("/panel/lokasyon") == true;


        public async Task<Location?> GetLocationByIdAsync(Guid id)
        {
            Console.Write("asdghfdhgfdhgfdhgsgfgsfd " + IsInLocationAdminPanel);
            return await _locationRepo.GetByIdAsync(id);
        }

        public async Task<ServiceCreateViewModel> GetServicesByLocationAsync(Guid locationId)
        {
            var model = new ServiceCreateViewModel
            {
                LocationId = locationId,
                ServiceList = (await _serviceRepo.GetByLocationIdAsync(locationId)).ToList(),
                AvailableWorkers = (await _workerRepo.GetByLocationIdAsync(locationId)).ToList()
            };

            return model;
        }

        public async Task<RequestResult> AddServiceToLocationAsync(ServiceCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Servis adı boş olamaz."
                };
            }

            var service = new Service
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Price = dto.Price,
                Duration = dto.Duration,
                ServiceGap = TimeSpan.FromMinutes(dto.ServiceGapMinutes),
                LocationId = dto.LocationId
            };

            // Servisi kaydet
            await _serviceRepo.AddAsync(service);

            // Eğer çalışan seçildiyse ekle
            if (dto.SelectedWorkerIds != null && dto.SelectedWorkerIds.Any())
            {
                var serviceEmployees = dto.SelectedWorkerIds.Select(workerId => new ServiceEmployee
                {
                    Id = Guid.NewGuid(),
                    ServiceId = service.Id,
                    WorkerId = workerId
                }).ToList();

                await _serviceEmployeeRepo.AddRangeAsync(serviceEmployees);
            }

            return new RequestResult
            {
                Success = true,
                Message = "Servis başarıyla eklendi."
            };
        }

        public async Task<RequestResult> DeleteServiceAsync(Guid id)
        {
            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null)
                return new RequestResult { Success = false, Message = "Silinmek istenen servis bulunamadı." };

            await _serviceRepo.DeleteAsync(service.Id);
            return new RequestResult { Success = true, Message = "Servis başarıyla silindi." };
        }

        public async Task<Service?> GetServiceByIdAsync(Guid id)
        {
            return await _serviceRepo.GetByIdWithDetailsAsync(id);
        }

        public async Task<RequestResult> UpdateServiceAsync(ServiceUpdateDto dto)
        {
            // Servisi veritabanından getir
            var service = await _serviceRepo.GetByIdWithDetailsAsync(dto.Id);
            if (service == null)
                return new RequestResult { Success = false, Message = "Güncellenecek servis bulunamadı." };

            // Temel bilgileri güncelle
            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Duration = dto.Duration;
            service.ServiceGap = TimeSpan.FromMinutes(dto.ServiceGapMinutes);

            // Servisi güncelle
            await _serviceRepo.UpdateAsync(service);

            // Mevcut tüm çalışanları temizle ve yeniden atama yap
            // Önce mevcut çalışanları listele
            var existingEmployees = await _serviceEmployeeRepo.GetByServiceIdAsync(service.Id);

            // Çıkarılan çalışanları sil
            foreach (var employee in existingEmployees)
            {
                if (!dto.SelectedWorkerIds.Contains(employee.WorkerId))
                {
                    await _serviceEmployeeRepo.DeleteAsync(employee.Id);
                }
            }

            // Yeni çalışanları ekle
            foreach (var workerId in dto.SelectedWorkerIds)
            {
                // Çalışan zaten bu servise atanmış mı kontrol et
                if (!existingEmployees.Any(e => e.WorkerId == workerId))
                {
                    var serviceEmployee = new ServiceEmployee
                    {
                        ServiceId = service.Id,
                        WorkerId = workerId
                    };
                    await _serviceEmployeeRepo.AddAsync(serviceEmployee);
                }
            }

            return new RequestResult { Success = true, Message = "Servis başarıyla güncellendi." };
        }

        public async Task<ServiceEmployee?> GetServiceEmployeeAsync(Guid serviceId, Guid workerId)
        {
            return await _serviceEmployeeRepo.GetByServiceAndWorkerIdAsync(serviceId, workerId);
        }

        public async Task<RequestResult> RemoveWorkerFromServiceAsync(Guid serviceEmployeeId)
        {
            try
            {
                await _serviceEmployeeRepo.DeleteAsync(serviceEmployeeId);

                return new RequestResult
                {
                    Success = true,
                    Message = "Çalışan bu servisten başarıyla kaldırıldı."
                };
            }
            catch (Exception)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan servisten kaldırılırken bir hata oluştu."
                };
            }
        }

        public async Task<bool> LocationExistsAsync(Guid locationId)
        {
            var location = await _locationRepo.GetByIdAsync(locationId);
            return location != null;
        }

        public async Task<IEnumerable<Worker>> GetAllWorkersAsync(Guid locationId)
        {
            return await _workerRepo.GetByLocationIdAsync(locationId);
        }

        public async Task<IEnumerable<WorkerWithReviewsDto>> GetWorkersWithReviewsAsync(Guid locationId)
        {
            // Get all workers for this location
            var workers = await _workerRepo.GetByLocationIdAsync(locationId);
            var workerDtos = workers.Select(WorkerWithReviewsDto.FromWorker).ToList();

            // For each worker, fetch their review stats
            foreach (var workerDto in workerDtos)
            {
                var stats = await _reviewRepo.GetWorkerReviewStatsAsync(workerDto.Id);
                workerDto.ReviewCount = stats.Count;
                workerDto.AverageRating = stats.AverageRating;
            }

            return workerDtos;
        }

        public async Task<RequestResult> AddWorkerToLocationAsync(WorkerAddDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Ad, soyad ve e-posta adresi boş olamaz."
                };
            }

            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            Guid userId;
            string? generatedPassword = null;

            if (existingUser != null)
            {
                userId = existingUser.Id;

                var existingWorker = await _workerRepo.GetByUserIdAsync(userId);

                // Check if the worker already exists in any location
                if (existingWorker != null)
                {
                    // Check if the worker is already in the current location
                    if (existingWorker.LocationId == dto.LocationId)
                    {
                        return new RequestResult
                        {
                            Success = false,
                            Message = "Bu kullanıcı zaten bu lokasyonda çalışıyor."
                        };
                    }
                    else
                    {
                        return new RequestResult
                        {
                            Success = false,
                            Message = "Bu kullanıcı zaten başka bir lokasyonda çalışıyor."
                        };
                    }
                }

                // Use the existing user's name as the worker's name
                dto.FullName = existingUser.FullName;
            }
            else
            {
                generatedPassword = Guid.NewGuid().ToString("N")[..8];
                var role = await _roleRepo.GetByNameAsync("Employee");

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(generatedPassword),
                    IsEmailConfirmed = false,
                    RoleId = role!.Id,
                };

                try
                {
                    await _userRepo.AddAsync(newUser);
                }
                catch
                {
                    return new RequestResult
                    {
                        Success = false,
                        Message = "Kullanıcı eklenirken bir hata oluştu."
                    };
                }
                userId = newUser.Id;
            }

            var newWorker = new Worker
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationId = dto.LocationId,
                RoleInLocation = dto.RoleInLocation
            };

            var location = await _locationRepo.GetByIdAsync(dto.LocationId);
            var companyName = location?.Name ?? "Firma";
            var fullName = dto.FullName;
            var roleDisplayName = EnumHelper.GetDisplayName(dto.RoleInLocation);

            try
            {
                await _workerRepo.AddAsync(newWorker);

                if (existingUser == null)
                {
                    await NotifyNewAccountWithAssignmentAsync(dto.Email, generatedPassword!, companyName, fullName, roleDisplayName);
                    return new RequestResult { Success = true, Message = "Çalışan başarıyla eklendi ve giriş bilgileri ilgili email adresine gönderildi." };
                }
                else
                {
                    await NotifyRoleAssignmentForExistingUserAsync(dto.Email, companyName, fullName, roleDisplayName);
                    return new RequestResult { Success = true, Message = "Çalışan başarıyla eklendi." };
                }
            }
            catch
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan eklenirken bir hata oluştu."
                };
            }
        }

        public async Task<RequestResult> DeleteWorkerAsync(Guid id, Guid currentUserId)
        {
            var worker = await _workerRepo.GetByIdWithDetailsAsync(id);
            if (worker == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan bulunamadı."
                };
            }

            // Only perform self-deletion check if currentUserId is not empty
            if (currentUserId != Guid.Empty && worker.User.Id == currentUserId)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Kendinizi silemezsiniz. Bu işlem için sistem yöneticisi ile iletişime geçin."
                };
            }

            try
            {
                // Delete the worker directly without sending email
                await _workerRepo.DeleteAsync(id);

                return new RequestResult
                {
                    Success = true,
                    Message = "Çalışan başarıyla silindi."
                };
            }
            catch (Exception ex)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = $"Çalışan silinirken bir hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<RequestResult> UpdateWorkerAsync(Guid id, WorkerAddDto dto)
        {
            var worker = await _workerRepo.GetByIdWithDetailsAsync(id);
            if (worker == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan bulunamadı."
                };
            }

            var oldName = worker.User.FullName;
            var oldPosition = EnumHelper.GetDisplayName(worker.RoleInLocation);
            var position = EnumHelper.GetDisplayName(dto.RoleInLocation);
            var email = worker.User.Email;
            var locationName = worker.Location.Name;

            worker.User.FullName = dto.FullName;
            worker.RoleInLocation = dto.RoleInLocation;

            try
            {
                await _workerRepo.UpdateAsync(worker);
                await NotifyWorkerUpdateAsync(email, locationName, dto.FullName, position, oldName, oldPosition);
                return new RequestResult
                {
                    Success = true,
                    Message = "Çalışan başarıyla güncellendi."
                };
            }
            catch (Exception ex)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = $"Çalışan güncellenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<Worker?> GetWorkerByIdAsync(Guid id)
        {
            return await _workerRepo.GetByIdWithDetailsAsync(id);
        }


        // WorkerHours
        public async Task<List<WorkerWorkingHour>> GetWorkerWorkingHoursAsync(Guid workerId, int year, int month)
        {
            return await _workerWorkingHourRepo.GetWorkerWorkingHoursAsync(workerId, year, month);
        }

        public async Task<RequestResult> AddWorkerWorkingHourAsync(WorkerWorkingHourDto dto)
        {
            var worker = await _workerRepo.GetByIdAsync(dto.WorkerId);

            if (worker == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan bulunamadı."
                };
            }

            // İş kuralları
            if (!dto.IsOnLeave && !dto.IsDayOff)
            {
                if (!dto.StartTime.HasValue || !dto.EndTime.HasValue)
                {
                    return new RequestResult
                    {
                        Success = false,
                        Message = "Çalışma günü için başlangıç ve bitiş saati zorunludur."
                    };
                }

                if (dto.StartTime.Value >= dto.EndTime.Value)
                {
                    return new RequestResult
                    {
                        Success = false,
                        Message = "Başlangıç saati, bitiş saatinden önce olmalıdır."
                    };
                }
            }

            List<DateTime> datesToProcess;

            if (!string.IsNullOrEmpty(dto.SelectedDays))
            {
                datesToProcess = System.Text.Json.JsonSerializer.Deserialize<List<DateTime>>(dto.SelectedDays) ?? [];
            }
            else
            {
                datesToProcess = [dto.Date];
            }

            var addList = new List<WorkerWorkingHour>();
            var updateList = new List<WorkerWorkingHour>();

            foreach (var rawDate in datesToProcess)
            {
                var normalizedDate = DateTime.SpecifyKind(rawDate, DateTimeKind.Unspecified);

                var existingWorkingHour = await _workerWorkingHourRepo.GetByWorkerIdAndDateAsync(dto.WorkerId, normalizedDate);

                if (existingWorkingHour != null)
                {
                    existingWorkingHour.StartTime = dto.StartTime;
                    existingWorkingHour.EndTime = dto.EndTime;
                    existingWorkingHour.IsOnLeave = dto.IsOnLeave;
                    existingWorkingHour.IsDayOff = dto.IsDayOff;

                    updateList.Add(existingWorkingHour);
                }
                else
                {
                    var newWorkingHour = new WorkerWorkingHour
                    {
                        Id = Guid.NewGuid(),
                        WorkerId = dto.WorkerId,
                        Date = normalizedDate,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        IsOnLeave = dto.IsOnLeave,
                        IsDayOff = dto.IsDayOff,
                        CreatedAt = DateTime.UtcNow
                    };

                    addList.Add(newWorkingHour);
                }
            }

            if (addList.Any())
                await _workerWorkingHourRepo.AddRangeAsync(addList);

            if (updateList.Any())
                await _workerWorkingHourRepo.UpdateRangeAsync(updateList);

            return new RequestResult
            {
                Success = true,
                Message = "Çalışma saatleri başarıyla kaydedildi."
            };
        }



        // Helper methods for email notifications
        private async Task NotifyNewAccountWithAssignmentAsync(string email, string password, string companyName, string fullName, string position)
        {

            var htmlBody = EmailTemplateHelper.LoadTemplate("NewWorkerEmail", new Dictionary<string, string>
                {
                    { "companyName", companyName },
                    { "fullname", fullName },
                    { "position", position },
                    { "email", email },
                    { "password", password },
                    { "loginUrl", EmailHelper.BaseUrl+"/Auth/Login" }
                });

            await _emailSender.SendEmailAsync(
                 email,
                 "Booknix • Firma Tarafından Hesabınız Oluşturuldu",
                 htmlBody,
                 "Booknix Destek"
             );

        }

        private async Task NotifyRoleAssignmentForExistingUserAsync(string email, string companyName, string fullName, string position)
        {

            var htmlBody = EmailTemplateHelper.LoadTemplate("WorkerPositionChange", new Dictionary<string, string>
                {
                    { "companyName", companyName },
                    { "fullname", fullName },
                    { "position", position },
                    { "assignedDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "profileUrl", EmailHelper.BaseUrl+"/Account/Manage" }
                });

            await _emailSender.SendEmailAsync(
                 email,
                 "Booknix • Hesap Üzerinde Yeni Atama",
                 htmlBody,
                 "Booknix Destek"
             );

        }

        private async Task NotifyWorkerRemovalAsync(string email, string companyName, string fullName, string position)
        {
            var htmlBody = EmailTemplateHelper.LoadTemplate("WorkerDelete", new Dictionary<string, string>
                {
                    { "companyName", companyName },
                    { "fullname", fullName },
                    { "position", position },
                    { "removalDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                });

            await _emailSender.SendEmailAsync(
                 email,
                 "Booknix • Firma Çalışanlığı Sonlandırıldı",
                 htmlBody,
                 "Booknix Destek"
             );

        }

        private async Task NotifyWorkerUpdateAsync(string email, string companyName, string fullName, string position, string oldName, string oldPosition)
        {

            var htmlBody = EmailTemplateHelper.LoadTemplate("WorkerUpdateTemplate", new Dictionary<string, string>
                {
                    { "companyName", companyName },
                    { "newFullName", fullName },
                    { "newPosition", position },
                    { "updateDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "oldFullName", oldName },
                    { "oldPosition", oldPosition },
                });

            await _emailSender.SendEmailAsync(
                 email,
                 "Booknix • Çalışan Bilgileriniz Güncellendi",
                 htmlBody,
                 "Booknix Destek"
             );

        }

    }
}
