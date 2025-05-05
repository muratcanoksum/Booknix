using System.Threading.Tasks;

namespace Booknix.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
