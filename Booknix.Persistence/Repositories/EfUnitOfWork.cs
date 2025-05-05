using Booknix.Domain.Interfaces;
using Booknix.Persistence.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Booknix.Persistence.Repositories
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly BooknixDbContext _context;
        private IDbContextTransaction? _transaction;

        public EfUnitOfWork(BooknixDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }
    }
}
