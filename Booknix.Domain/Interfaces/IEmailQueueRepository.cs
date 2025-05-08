using Booknix.Domain.Entities;
using Booknix.Domain.Entities.Enums;

namespace Booknix.Domain.Interfaces
{
    public interface IEmailQueueRepository
    {
        Task AddAsync(EmailQueue email);
        Task<List<EmailQueue>> GetPendingAsync(int limit = 10);
        Task UpdateAsync(EmailQueue email);
        Task<List<EmailQueue>> GetAllAsync();
        Task<List<EmailQueue>> GetByStatusAsync(EmailQueueStatus status);
        Task<EmailQueue?> GetByIdAsync(Guid id);

    }
}
