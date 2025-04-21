using Booknix.Application.Interfaces;
using Booknix.Application.DTOs;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;


namespace Booknix.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly ISectorRepository _sectorRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IWorkerRepository _workerRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;

        public AdminService(ISectorRepository sectorRepo, ILocationRepository locationRepo, IServiceRepository serviceRepo, IWorkerRepository workerRepo, IUserRepository userRepo, IRoleRepository roleRepo)
        {
            _sectorRepo = sectorRepo;
            _locationRepo = locationRepo;
            _serviceRepo = serviceRepo;
            _workerRepo = workerRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
        }

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
            return false;
        }


        // Workers

        public async Task<IEnumerable<Worker>> GetAllWorkersAsync(Guid locationId)
        {
            return await _workerRepo.GetByLocationIdAsync(locationId);
        }

        public async Task<RequestResult> AddWorkerToLocationAsync(WorkerAddDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.Email))
                return new RequestResult
                {
                    Success = false,
                    Message = "Ad, soyad ve e-posta adresi boş olamaz."
                };

            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);

            Guid userId;

            if (existingUser != null)
            {
                // Kullanıcı zaten varsa
                userId = existingUser.Id;

                // Bu kullanıcı başka bir lokasyonda zaten çalışıyor mu?
                var alreadyWorker = await _workerRepo.GetByUserIdAsync(userId);
                if (alreadyWorker != null)
                    return new RequestResult
                    {
                        Success = false,
                        Message = "Bu kullanıcı zaten başka bir lokasyonda çalışıyor."
                    };
            }
            else
            {
                // Yeni kullanıcı oluşturulacak
                var randomPassword = Guid.NewGuid().ToString("N")[..8]; // 8 karakterlik şifre
                Console.WriteLine($"[Yeni Kullanıcı Şifresi] {randomPassword}");

                var role = await _roleRepo.GetByNameAsync("Client");

                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword),
                    IsEmailConfirmed = false,
                    RoleId = role!.Id,
                };

                await _userRepo.AddAsync(newUser);

                userId = newUser.Id;
            }

            // Worker kaydı oluşturuluyor
            var newWorker = new Worker
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LocationId = dto.LocationId,
                RoleInLocation = dto.RoleInLocation
            };

            await _workerRepo.AddAsync(newWorker);

            return new RequestResult { Success = true, Message = "Çalışan başarıyla eklendi." };
        }


        //BUNDA KALDIN ÜSTTEKİNDEDE HATALAR VAR
        public async Task<RequestResult> DeleteWorkerAsync(Guid id)
        {
            // 1. Çalışan var mı kontrol et
            var worker = await _workerRepo.GetByIdAsync(id);
            if (worker == null)
            {
                return new RequestResult
                {
                    Success = false,
                    Message = "Çalışan bulunamadı."
                };
            }

            // 2. Silme işlemi
            await _workerRepo.DeleteAsync(id);

            return new RequestResult
            {
                Success = true,
                Message = "Çalışan başarıyla silindi."
            };
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

    }
}
