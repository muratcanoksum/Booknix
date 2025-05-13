using Booknix.Application.Interfaces;
using Booknix.Application.DTOs;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Shared.Helpers;
using Booknix.Application.Helpers;
using Booknix.Domain.Entities.Enums;
using Booknix.Application.ViewModels;
using System.Text.Json;


namespace Booknix.Application.Services
{
    public class AdminService(
        ISectorRepository sectorRepo,
        ILocationRepository locationRepo,
        IServiceRepository serviceRepo,
        IWorkerRepository workerRepo,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IAuditLogger auditLogger,
        IEmailSender emailSender,
        IServiceEmployeeRepository serviceEmployeeRepo,
        IWorkerWorkingHourRepository workerWorkingHourRepo,
        IUserSessionRepository userSessionRepo,
        IEmailQueueRepository emailQueueRepo,
        IEmailQueueNotifier emailQueueNotifier,
        IReviewRepository reviewRepo
            ) : IAdminService
    {
        private readonly ISectorRepository _sectorRepo = sectorRepo;
        private readonly ILocationRepository _locationRepo = locationRepo;
        private readonly IServiceRepository _serviceRepo = serviceRepo;
        private readonly IWorkerRepository _workerRepo = workerRepo;
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly IAuditLogger _auditLogger = auditLogger;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IServiceEmployeeRepository _serviceEmployeeRepo = serviceEmployeeRepo;
        private readonly IWorkerWorkingHourRepository _workerWorkingHourRepo = workerWorkingHourRepo;
        private readonly IUserSessionRepository _userSessionRepo = userSessionRepo;
        private readonly IEmailQueueRepository _emailQueueRepo = emailQueueRepo;
        private readonly IEmailQueueNotifier _emailQueueNotifier = emailQueueNotifier;
        private readonly IReviewRepository _reviewRepo = reviewRepo;

        // Sectors

        public async Task<IEnumerable<Sector>> GetAllSectorsAsync()
        {
            var sectors = await _sectorRepo.GetAllAsync();
            return sectors;
        }

        public async Task<RequestResult> AddSectorAsync(string name)
        {
            // 1. Geçerli bir ad girilmiş mi kontrol et
            if (string.IsNullOrWhiteSpace(name))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Sektör adı boş olamaz."
                };
            }

            // 2. Aynı isimde sektör zaten var mı kontrol et
            var existingSector = await _sectorRepo.GetByNameAsync(name);
            if (existingSector != null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Bu adda bir sektör zaten mevcut."
                };
            }

            // 3. Yeni sektör oluştur ve kaydet
            var newSector = new Sector
            {
                Name = name.Trim(),
                Slug = GenerateSlug(name),
            };

            await _sectorRepo.AddAsync(newSector);

            return new RequestResult
            {
                Success = true,
                Message = "Sektör başarıyla eklendi."
            };
        }

        public async Task<RequestResult> DeleteSectorAsync(Guid id)
        {
            var sector = await _sectorRepo.GetByIdAsync(id);

            if (sector == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Silinmek istenen sektör bulunamadı."
                };
            }

            await _sectorRepo.DeleteAsync(sector.Id);

            return new RequestResult
            {
                Success = true,
                Message = "Sektör başarıyla silindi."
            };
        }

        public async Task<Sector?> GetSectorByIdAsync(Guid id)
        {
            var sector = await _sectorRepo.GetByIdAsync(id);

            if (sector == null)
            {
                return null;
            }

            return sector;
        }

        public async Task<RequestResult> UpdateSectorAsync(Guid id, string name)
        {
            var sector = await _sectorRepo.GetByIdAsync(id);

            if (sector == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Güncellenecek sektör bulunamadı."
                };
            }

            // İsim aynıysa (değişiklik yok)
            if (sector.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Sektör bilgileri zaten güncel."
                };
            }

            // Aynı isim başka bir sektörde varsa çakışmayı engelle
            var existing = await _sectorRepo.GetByNameAsync(name);
            if (existing != null && existing.Id != sector.Id)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Bu isimde başka bir sektör zaten mevcut."
                };
            }

            // Güncelleme işlemi
            sector.Name = name.Trim();
            sector.Slug = GenerateSlug(name);
            await _sectorRepo.UpdateAsync(sector);

            return new RequestResult
            {
                Success = true,
                Message = "Sektör başarıyla güncellendi."
            };
        }

        //

        private static string GenerateSlug(string text)
        {
            var turkishMap = new Dictionary<char, char>
            {
                ['ç'] = 'c',
                ['Ç'] = 'c',
                ['ğ'] = 'g',
                ['Ğ'] = 'g',
                ['ı'] = 'i',
                ['İ'] = 'i',
                ['ö'] = 'o',
                ['Ö'] = 'o',
                ['ş'] = 's',
                ['Ş'] = 's',
                ['ü'] = 'u',
                ['Ü'] = 'u'
            };

            var normalized = text
                .Trim()
                .ToLower()
                .Replace("&", " and ")
                .Select(c => turkishMap.ContainsKey(c) ? turkishMap[c] : c)
                .ToArray();

            var slug = new string(normalized)
                .Replace(" ", "-")
                .Replace("--", "-")
                .Replace("---", "-");

            return slug;
        }





        // Locations
        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _locationRepo.GetAllAsync();
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
                SectorId = sectorId,
                Slug = GenerateSlug(name)
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
            location.Slug = GenerateSlug(name.Trim());

            await _locationRepo.UpdateAsync(location);

            return new RequestResult
            {
                Success = true,
                Message = "Lokasyon başarıyla güncellendi."
            };
        }




        // Services

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


        // Workers

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

                if (!string.Equals(existingUser.FullName, dto.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    return new RequestResult
                    {
                        Success = false,
                        Message = "Bu mail adresine kayıtlı başka bir üyemiz bulunmakta lütfen destek ile iletişime geçin."
                    };
                }

                if (existingWorker != null)
                {
                    return new RequestResult
                    {
                        Success = false,
                        Message = "Bu kullanıcı zaten başka bir lokasyonda çalışıyor."
                    };
                }
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
                // 2. Silme işlemi
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

        public async Task<RequestResult> UpdateWorkerAsync(Guid id, WorkerAddDto dto)
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan güncellenirken hata oluştu lütfen sistem yöneticinize başvurun."
                };

            }

        }

        public async Task<Worker?> GetWorkerByIdAsync(Guid id)
        {
            await Task.Delay(1000); // Simulate some delay
            return null; // Simulate not found
        }

        //YARDIMCI FONKSİYONLAR

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

        //YARDIMCI FONKSİYONLAR

        // Worker Hour

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
                datesToProcess = JsonSerializer.Deserialize<List<DateTime>>(dto.SelectedDays) ?? [];
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

        // User

        public async Task<List<User>> GetAllUsersAsync()
        {
            return (await _userRepo.GetAllAsync()).ToList();
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return (await _roleRepo.GetAllAsync()).ToList();
        }

        public async Task<RequestResult> UpdateUserAsync(UserUpdateDto model)
        {
            if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Ad ve e-posta alanları boş bırakılamaz."
                };
            }

            var user = await _userRepo.GetByIdAsync(model.Id);
            if (user == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Kullanıcı bulunamadı."
                };
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.RoleId = model.RoleId;

            try
            {
                await _userRepo.UpdateAsync(user);
                return new RequestResult
                {
                    Success = true,
                    Message = "Kullanıcı başarıyla güncellendi."
                };
            }
            catch (Exception)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Güncelleme sırasında hata oluştu."
                };
            }
        }

        public async Task<RequestResult> DeleteUserAsync(Guid id, string currentUserId)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Kullanıcı bulunamadı."
                };
            }

            if (currentUserId == id.ToString() && user.Role?.Name == "Admin")
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Kendinizi silemezsiniz."
                };
            }

            try
            {
                await _userRepo.DeleteAsync(user);
                return new RequestResult
                {
                    Success = true,
                    Message = "Kullanıcı başarıyla silindi."
                };
            }
            catch (Exception)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Silme işlemi sırasında hata oluştu."
                };
            }
        }

        public async Task<RequestResult> CreateUserAsync(UserCreateDto model)
        {
            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Ad, e-posta ve şifre alanları boş bırakılamaz."
                };
            }

            var existingUser = await _userRepo.GetByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Bu e-posta adresi zaten kullanılıyor."
                };
            }

            var role = await _roleRepo.GetByIdAsync(model.RoleId);
            if (role == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Geçersiz rol seçildi."
                };
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RoleId = model.RoleId,
                IsEmailConfirmed = true
            };

            try
            {
                await _userRepo.AddAsync(user);
                return new RequestResult
                {
                    Success = true,
                    Message = "Kullanıcı başarıyla oluşturuldu."
                };
            }
            catch (Exception)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Kullanıcı oluşturulurken bir hata oluştu."
                };
            }
        }

        // Session
        public async Task<List<UserSession>> GetActiveSessionsAsync(Guid userId)
        {
            return await _userSessionRepo.GetActiveSessionsByUserIdAsync(userId);
        }

        public async Task<RequestResult> TerminateSessionAsync(Guid UserId, string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Geçersiz oturum anahtarı."
                };
            }

            try
            {
                await _userSessionRepo.DeactivateBySessionKeyAsync(UserId, sessionKey);
                return new RequestResult
                {
                    Success = true,
                    Message = "Oturum sonlandırıldı."
                };
            }
            catch (Exception)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Oturum sonlandırılırken hata oluştu."
                };
            }
        }

        public async Task<RequestResult> TerminateAllSessionsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Geçersiz kullanıcı."
                };
            }

            try
            {
                await _userSessionRepo.DeactivateAllByUserIdAsync(userId);
                return new RequestResult
                {
                    Success = true,
                    Message = "Tüm oturumlar sonlandırıldı."
                };
            }
            catch (Exception)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Oturumlar sonlandırılırken hata oluştu."
                };
            }
        }


        // EmailQueue
        public async Task<List<EmailQueue>> GetAllEmailQueuesAsync()
        {
            return await _emailQueueRepo.GetAllAsync();
        }

        public async Task<List<EmailQueue>> GetEmailsByStatusAsync(EmailQueueStatus status)
        {
            return await _emailQueueRepo.GetByStatusAsync(status);
        }

        public async Task<RequestResult> CancelEmailAsync(Guid id)
        {
            var email = await _emailQueueRepo.GetByIdAsync(id);
            if (email == null)
                return new RequestResult(false, "Kuyruk işlemi bulunamadı.");

            var oldStatus = email.Status.ToString();
            email.Status = EmailQueueStatus.Cancelled;
            email.UpdatedAt = DateTime.UtcNow;
            await _emailQueueRepo.UpdateAsync(email);

            await _emailQueueNotifier.NotifyStatusChangedAsync(email, oldStatus);

            return new RequestResult(true, "E-posta başarıyla iptal edildi.");
        }


        public async Task<RequestResult> RetryEmailAsync(Guid id)
        {
            var email = await _emailQueueRepo.GetByIdAsync(id);
            if (email == null)
                return new RequestResult(false, "Kuyruk işlemi bulunamadı.");

            var oldStatus = email.Status.ToString();
            email.Status = EmailQueueStatus.Pending;
            email.ErrorMessage = null;
            email.SentAt = null;
            email.UpdatedAt = DateTime.UtcNow;
            await _emailQueueRepo.UpdateAsync(email);

            await _emailQueueNotifier.NotifyStatusChangedAsync(email, oldStatus);

            return new RequestResult(true, "E-posta yeniden kuyruğa alındı.");
        }



    }
}
