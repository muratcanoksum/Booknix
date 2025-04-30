using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Application.ViewModels;
using Booknix.Shared.Helpers;

namespace Booknix.Application.Services
{
    public class LocationAdminService(
        ILocationRepository locationRepo,
        IServiceRepository serviceRepo,
        IWorkerRepository workerRepo,
        IServiceEmployeeRepository serviceEmployeeRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IEmailSender emailSender
        ) : ILocationAdminService
    {
        private readonly ILocationRepository _locationRepo = locationRepo;
        private readonly IServiceRepository _serviceRepo = serviceRepo;
        private readonly IWorkerRepository _workerRepo = workerRepo;
        private readonly IServiceEmployeeRepository _serviceEmployeeRepo = serviceEmployeeRepo;
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly IEmailSender _emailSender = emailSender;

        // Locations
        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _locationRepo.GetAllAsync();
        }
        
        
        
        public async Task<List<Service>> GetServicesByLocationAsync(Guid locationId)
        {
            var services = await _serviceRepo.GetByLocationIdAsync(locationId);
            return services.ToList();
        }

        public async Task<RequestResult> AddLocationAsync(string name, string address, string phoneNumber, Guid sectorId)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Lokasyon adı, adres ve telefon numarası boş olamaz."
                };
            }

            var existing = await _locationRepo.GetByNameAsync(name);
            if (existing != null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Bu isimde bir lokasyon zaten mevcut."
                };
            }

            var location = new Location
            {
                Id = Guid.NewGuid(),
                Name = name.Trim(),
                Address = address.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                SectorId = sectorId
            };

            await _locationRepo.AddAsync(location);

            return new RequestResult
            {
                Success = true,
                Message = "Lokasyon başarıyla eklendi."
            };
        }

        public async Task<RequestResult> DeleteLocationAsync(Guid id)
        {
            var location = await _locationRepo.GetByIdAsync(id);
            if (location == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Silinecek lokasyon bulunamadı."
                };
            }

            await _locationRepo.DeleteAsync(id);

            return new RequestResult
            {
                Success = true,
                Message = "Lokasyon başarıyla silindi."
            };
        }

        public async Task<Location?> GetLocationByIdAsync(Guid id)
        {
            return await _locationRepo.GetByIdAsync(id);
        }

        public async Task<RequestResult> UpdateLocationAsync(Guid id, string name, string address, string phoneNumber, Guid sectorId)
        {
            var location = await _locationRepo.GetByIdAsync(id);
            if (location == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Güncellenecek lokasyon bulunamadı."
                };
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Lokasyon adı, adres ve telefon numarası boş olamaz."
                };
            }

            // Güncelleme gerekmediği durumda
            if (location.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                location.Address.Equals(address, StringComparison.OrdinalIgnoreCase) &&
                location.PhoneNumber.Equals(phoneNumber, StringComparison.OrdinalIgnoreCase) &&
                location.SectorId == sectorId)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Lokasyon bilgileri zaten güncel."
                };
            }

            // Aynı isme sahip başka bir lokasyon var mı?
            var existing = await _locationRepo.GetByNameAsync(name);
            if (existing != null && existing.Id != location.Id)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Bu isimde başka bir lokasyon zaten mevcut."
                };
            }

            location.Name = name.Trim();
            location.Address = address.Trim();
            location.PhoneNumber = phoneNumber.Trim();
            location.SectorId = sectorId;

            await _locationRepo.UpdateAsync(location);

            return new RequestResult
            {
                Success = true,
                Message = "Lokasyon başarıyla güncellendi."
            };
        }

        public async Task<ServiceCreateViewModel> GetServiceCreateViewModelAsync(Guid locationId)
        {
            var model = new ServiceCreateViewModel
            {
                LocationId = locationId,
                ServiceList = (await _serviceRepo.GetByLocationIdAsync(locationId)).ToList(),
                AvailableWorkers = (await _workerRepo.GetByLocationIdAsync(locationId)).ToList()
            };

            return model;
        }

        public async Task<Service?> GetServiceByIdAsync(Guid id)
        {
            return await _serviceRepo.GetByIdWithDetailsAsync(id);
        }

        public async Task<RequestResult> AddServiceAsync(ServiceCreateDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
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
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                Price = model.Price,
                Duration = model.Duration,
                LocationId = model.LocationId
            };

            await _serviceRepo.AddAsync(service);

            if (model.SelectedWorkerIds != null && model.SelectedWorkerIds.Any())
            {
                var serviceEmployees = model.SelectedWorkerIds.Select(workerId => new ServiceEmployee
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

        public async Task<RequestResult> UpdateServiceAsync(ServiceUpdateDto model)
        {
            var service = await _serviceRepo.GetByIdAsync(model.Id);
            if (service == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Güncellenecek servis bulunamadı."
                };
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Servis adı boş olamaz."
                };
            }

            service.Name = model.Name.Trim();
            service.Description = model.Description?.Trim();
            service.Price = model.Price;
            service.Duration = model.Duration;

            await _serviceRepo.UpdateAsync(service);

            // Update service workers if provided
            if (model.SelectedWorkerIds != null && model.SelectedWorkerIds.Any())
            {
                // First remove all existing workers
                var existingServiceEmployees = await _serviceEmployeeRepo.GetByServiceIdAsync(service.Id);
                foreach (var employee in existingServiceEmployees)
                {
                    await _serviceEmployeeRepo.DeleteAsync(employee.Id);
                }

                // Then add the new workers
                var serviceEmployees = model.SelectedWorkerIds.Select(workerId => new ServiceEmployee
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
                Message = "Servis başarıyla güncellendi."
            };
        }

        public async Task<RequestResult> DeleteServiceAsync(Guid id)
        {
            var service = await _serviceRepo.GetByIdAsync(id);
            if (service == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Silinecek servis bulunamadı."
                };
            }

            await _serviceRepo.DeleteAsync(id);

            return new RequestResult
            {
                Success = true,
                Message = "Servis başarıyla silindi."
            };
        }

        public async Task<RequestResult> RemoveWorkerFromServiceAsync(Guid serviceId, Guid workerId)
        {
            var serviceEmployee = await _serviceEmployeeRepo.GetByServiceAndWorkerIdAsync(serviceId, workerId);
            if (serviceEmployee == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan bu serviste bulunamadı."
                };
            }

            await _serviceEmployeeRepo.DeleteAsync(serviceEmployee.Id);

            return new RequestResult
            {
                Success = true,
                Message = "Çalışan servisten başarıyla kaldırıldı."
            };
        }

        public async Task<IEnumerable<Worker>> GetWorkersByLocationAsync(Guid locationId)
        {
            return await _workerRepo.GetByLocationIdAsync(locationId);
        }

        public async Task<Worker?> GetWorkerByIdAsync(Guid id)
        {
            return await _workerRepo.GetByIdAsync(id);
        }

        public async Task<RequestResult> AddWorkerAsync(WorkerAddDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "E-posta adresi boş olamaz."
                };
            }

            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            Guid userId;
            string? generatedPassword = null;

            if (existingUser != null)
            {
                userId = existingUser.Id;
                var existingWorker = await _workerRepo.GetByUserIdAsync(userId);

                if (existingWorker != null)
                {
                    return new RequestResult
                    {
                        Success = false,
                        Message = "Bu kullanıcı zaten başka bir lokasyonda çalışıyor."
                    };
                }

                // Mevcut kullanıcının adını kullan
                dto.FullName = existingUser.FullName;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.FullName))
                {
                    return new RequestResult
                    {
                        Success = false,
                        Message = "Yeni kullanıcı için ad ve soyad gereklidir."
                    };
                }

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

        public async Task<RequestResult> UpdateWorkerAsync(WorkerUpdateDto dto)
        {
            var worker = await _workerRepo.GetByIdAsync(dto.Id);
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

            worker.User.FullName = dto.FullName;
            worker.RoleInLocation = dto.RoleInLocation;

            try
            {
                await _workerRepo.UpdateAsync(worker);
                await NotifyWorkerUpdateAsync(worker.User.Email, worker.Location.Name, dto.FullName, position, oldName, oldPosition);
                return new RequestResult
                {
                    Success = true,
                    Message = "Çalışan başarıyla güncellendi."
                };
            }
            catch
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan güncellenirken hata oluştu lütfen sistem yöneticinize başvurun."
                };
            }
        }

        public async Task<RequestResult> DeleteWorkerAsync(Guid id)
        {
            var worker = await _workerRepo.GetByIdAsync(id);
            if (worker == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan bulunamadı."
                };
            }

            try
            {
                await _workerRepo.DeleteAsync(id);

                var roleDisplayName = EnumHelper.GetDisplayName(worker.RoleInLocation);
                await NotifyWorkerRemovalAsync(worker.User.Email, worker.Location.Name, worker.User.FullName, roleDisplayName);

                return new RequestResult
                {
                    Success = true,
                    Message = "Çalışan başarıyla silindi."
                };
            }
            catch
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan silinirken bir hata oluştu."
                };
            }
        }

        private async Task NotifyNewAccountWithAssignmentAsync(string email, string password, string companyName, string fullName, string position)
        {
            var html = EmailTemplateHelper.LoadTemplate("NewAccountWithAssignment", new Dictionary<string, string>
            {
                { "fullname", fullName },
                { "companyName", companyName },
                { "position", position },
                { "email", email },
                { "password", password }
            });

            await _emailSender.SendEmailAsync(email, "Yeni Hesap ve Pozisyon Ataması", html, "Booknix");
        }

        private async Task NotifyRoleAssignmentForExistingUserAsync(string email, string companyName, string fullName, string position)
        {
            var html = EmailTemplateHelper.LoadTemplate("RoleAssignmentForExistingUser", new Dictionary<string, string>
            {
                { "fullname", fullName },
                { "companyName", companyName },
                { "position", position }
            });

            await _emailSender.SendEmailAsync(email, "Yeni Pozisyon Ataması", html, "Booknix");
        }

        private async Task NotifyWorkerUpdateAsync(string email, string companyName, string fullName, string position, string oldName, string oldPosition)
        {
            var html = EmailTemplateHelper.LoadTemplate("WorkerUpdate", new Dictionary<string, string>
            {
                { "fullname", fullName },
                { "companyName", companyName },
                { "position", position },
                { "oldName", oldName },
                { "oldPosition", oldPosition }
            });

            await _emailSender.SendEmailAsync(email, "Çalışan Bilgileri Güncellendi", html, "Booknix");
        }

        private async Task NotifyWorkerRemovalAsync(string email, string companyName, string fullName, string position)
        {
            var html = EmailTemplateHelper.LoadTemplate("WorkerRemoval", new Dictionary<string, string>
            {
                { "fullname", fullName },
                { "companyName", companyName },
                { "position", position }
            });

            await _emailSender.SendEmailAsync(email, "Çalışan Kaydı Silindi", html, "Booknix");
        }
    }
}
