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

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> work)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            var result = await work();
            await tx.CommitAsync();
            return result; // disposing an uncommitted transaction (exception path) rolls it back
        }
    }
}
