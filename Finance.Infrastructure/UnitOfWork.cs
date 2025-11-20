using Finance.Application.Interfaces;
using Finance.Infrastructure.Data;

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
            await _context.SaveChangesAsync();
        }
    }
}
