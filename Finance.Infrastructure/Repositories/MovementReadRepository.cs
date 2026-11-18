using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using Finance.Domain.Models.Enums;
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

        public async Task<IReadOnlyList<Moviment>> ListAsync(
            DateOnly? from, DateOnly? toExclusive, int? categoryId, EMovimentType? type)
        {
            IQueryable<Moviment> q = _context.Moviments.AsNoTracking().Include(m => m.Category);
            if (from is not null) q = q.Where(m => m.MovimentDate >= from.Value);
            if (toExclusive is not null) q = q.Where(m => m.MovimentDate < toExclusive.Value);
            if (categoryId is not null) q = q.Where(m => m.CategoryId == categoryId.Value);
            if (type is not null) q = q.Where(m => m.Type == type.Value);
            return await q.OrderByDescending(m => m.MovimentDate).ThenByDescending(m => m.Id).ToListAsync();
        }

        public async Task<IReadOnlyList<CategoryTotalDto>> TotalsByCategoryAsync(DateOnly from, DateOnly toExclusive)
        {
            // Aggregated in SQL; groups exist only for categories with movements, so no empty sums.
            var rows = await _context.Moviments.AsNoTracking()
                .Where(m => m.MovimentDate >= from && m.MovimentDate < toExclusive)
                .GroupBy(m => new { m.CategoryId, m.Category!.Name, m.Type })
                .Select(g => new { g.Key.CategoryId, g.Key.Name, g.Key.Type, Total = g.Sum(m => m.Value) })
                .ToListAsync();
            return rows.OrderBy(r => r.Type).ThenBy(r => r.Name).ThenBy(r => r.CategoryId)
                .Select(r => new CategoryTotalDto(r.CategoryId, r.Name, r.Type, r.Total)).ToList();
        }

        public async Task<IReadOnlyList<MonthPointDto>> TotalsByMonthAsync(DateOnly from, DateOnly toExclusive)
        {
            var rows = await _context.Moviments.AsNoTracking()
                .Where(m => m.MovimentDate >= from && m.MovimentDate < toExclusive)
                .GroupBy(m => new { m.MovimentDate.Year, m.MovimentDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Income = g.Where(m => m.Type == EMovimentType.Income).Sum(m => (decimal?)m.Value) ?? 0m,
                    Expense = g.Where(m => m.Type == EMovimentType.Expense).Sum(m => (decimal?)m.Value) ?? 0m
                })
                .ToListAsync();
            return rows.OrderBy(r => r.Year).ThenBy(r => r.Month)
                .Select(r => new MonthPointDto(r.Year, r.Month, r.Income, r.Expense)).ToList();
        }
    }
}
