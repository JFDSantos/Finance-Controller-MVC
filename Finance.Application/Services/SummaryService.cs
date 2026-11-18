using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Finance.Domain.Models.Enums;

namespace Finance.Application.Services
{
    public class SummaryService : ISummaryService
    {
        private readonly IMovementReadRepository _repo;
        private readonly TimeProvider _time;

        public SummaryService(IMovementReadRepository repo, TimeProvider time)
        {
            _repo = repo;
            _time = time;
        }

        public async Task<MonthlySummaryDto> GetMonthlyAsync(int year, int month)
        {
            PeriodValidation.ValidateYear(year);
            PeriodValidation.ValidateMonth(month);

            var from = new DateOnly(year, month, 1);
            var totals = await _repo.TotalsByCategoryAsync(from, from.AddMonths(1));

            var income = totals.Where(t => t.Type == EMovimentType.Income).Sum(t => t.Total);
            var expense = totals.Where(t => t.Type == EMovimentType.Expense).Sum(t => t.Total);
            return new MonthlySummaryDto(year, month, income, expense, income - expense, totals);
        }

        public async Task<IReadOnlyList<MonthPointDto>> GetSeriesAsync(int months)
        {
            PeriodValidation.ValidateMonths(months);

            var now = _time.GetUtcNow();
            var current = new DateOnly(now.Year, now.Month, 1);
            var first = current.AddMonths(-(months - 1));
            var data = await _repo.TotalsByMonthAsync(first, current.AddMonths(1));
            var byKey = data.ToDictionary(p => (p.Year, p.Month));

            var result = new List<MonthPointDto>(months);
            for (var i = 0; i < months; i++)
            {
                var m = first.AddMonths(i);
                result.Add(byKey.TryGetValue((m.Year, m.Month), out var p)
                    ? p
                    : new MonthPointDto(m.Year, m.Month, 0m, 0m));
            }
            return result;
        }
    }
}
