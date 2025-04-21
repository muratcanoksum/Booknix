using Booknix.Application.DTOs;

namespace Booknix.Application.Interfaces
{
    public interface ISectorService
    {
        Task<IEnumerable<SectorDto>> GetAllAsync();
        Task<SectorDto?> GetByIdAsync(Guid id);
        Task AddAsync(SectorDto dto);
        Task UpdateAsync(SectorDto dto);
        Task DeleteAsync(Guid id);
    }
}
