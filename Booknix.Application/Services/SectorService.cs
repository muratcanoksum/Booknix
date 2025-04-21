using Booknix.Application.DTOs;
using Booknix.Application.Interfaces;
using Booknix.Domain.Entities;
using Booknix.Domain.Interfaces;

namespace Booknix.Application.Services
{
    public class SectorService : ISectorService
    {
        private readonly ISectorRepository _repo;

        public SectorService(ISectorRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<SectorDto>> GetAllAsync()
        {
            var sectors = await _repo.GetAllAsync();
            return sectors.Select(s => new SectorDto
            {
                Id = s.Id,
                Name = s.Name
            });
        }

        public async Task<SectorDto?> GetByIdAsync(Guid id)
        {
            var sector = await _repo.GetByIdAsync(id);
            if (sector == null) return null;

            return new SectorDto
            {
                Id = sector.Id,
                Name = sector.Name
            };
        }

        public async Task AddAsync(SectorDto dto)
        {
            var sector = new Sector
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };
            await _repo.AddAsync(sector);
        }

        public async Task UpdateAsync(SectorDto dto)
        {
            var sector = await _repo.GetByIdAsync(dto.Id);
            if (sector == null) throw new Exception("Sektör bulunamadý.");
            sector.Name = dto.Name;
            await _repo.UpdateAsync(sector);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
