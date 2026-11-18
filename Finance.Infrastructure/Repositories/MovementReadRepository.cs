using Finance.Application.Interfaces;
using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Repositories
{
    public class MovementReadRepository : IMovementReadRepository
    {
        protected readonly FinanceContext _context;

        public MovementReadRepository(FinanceContext context)
        {
            _context = context;
        }

        // Goes through the filtered DbSet, so only the current user's movements count.
        public Task<bool> AnyByCategoryAsync(int categoryId)
            => _context.Moviments.AnyAsync(m => m.CategoryId == categoryId);
    }
}
