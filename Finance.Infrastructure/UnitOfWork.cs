using Finance.Application.Exceptions;
using Finance.Application.Interfaces;
using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Finance.Application.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FinanceContext _context;

        public UnitOfWork(FinanceContext context)
        {
            _context = context;
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new UniqueConstraintException(ex);
            }
        }

        public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work)
        {
            // With EnableRetryOnFailure, user-initiated transactions must run inside the execution strategy.
            // The whole block (including `work`) is re-run on a transient failure, so `work` must create its
            // entities itself, and the change tracker is cleared so a retry never re-adds tracked entities.
            var strategy = _context.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();
                await using var tx = await _context.Database.BeginTransactionAsync();
                var result = await work();
                await tx.CommitAsync();
                return result; // disposing an uncommitted transaction (exception path) rolls it back
            });
        }
    }
}
