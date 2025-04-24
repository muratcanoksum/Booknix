using Booknix.Application.Interfaces;
using Booknix.Application.DTOs;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;
using Booknix.Shared.Interfaces;
using Booknix.Shared.Helpers;
using Booknix.Application.Helpers;
using Booknix.Domain.Entities.Enums;


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
        IEmailSender emailSender
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
                Name = name.Trim()
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
            await _sectorRepo.UpdateAsync(sector);

            return new RequestResult
            {
                Success = true,
                Message = "Sektör başarıyla güncellendi."
            };
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




        // Services

        public async Task<IEnumerable<Service>> GetServicesByLocationAsync(Guid locationId)
        {
            return await _serviceRepo.GetByLocationIdAsync(locationId);
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
                LocationId = dto.LocationId
            };

            await _serviceRepo.AddAsync(service);

            return new RequestResult
            {
                Success = true,
                Message = "Servis başarıyla eklendi."
            };
        }

        public async Task<RequestResult> DeleteServiceAsync(Guid id)
        {
            await _serviceRepo.DeleteAsync(id);

            return new RequestResult
            {
                Success = true,
                Message = "Servis başarıyla silindi."
            };
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

        public async Task<RequestResult> UpdateWorkerAsync(Guid id, string fullName, string email, int role, Guid locationId)
        {
            await Task.Delay(1000); // Simulate some delay
            return new RequestResult
            {
                Success = false,
                Message = "Bu özellik henüz geliştirilmedi."
            };
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

        //YARDIMCI FONKSİYONLAR


    }
}
