using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Finance.Domain.Models.Enums;

namespace Finance.Application.Services
{
    public class MovementQueryService : IMovementQueryService
    {
        private readonly IMovementReadRepository _repo;

        public MovementQueryService(IMovementReadRepository repo) => _repo = repo;

        public async Task<IReadOnlyList<MovementListItemDto>> ListAsync(
            int? year, int? month, int? categoryId, EMovimentType? type)
        {
            DateOnly? from = null, to = null;
            if (month is not null)
            {
                PeriodValidation.ValidateMonth(month);
                PeriodValidation.ValidateYear(year); // month without year is invalid
            }
            if (year is not null)
            {
                PeriodValidation.ValidateYear(year);
                if (month is null)
                {
                    from = new DateOnly(year.Value, 1, 1);
                    to = from.Value.AddYears(1);
                }
                else
                {
                    from = new DateOnly(year.Value, month.Value, 1);
                    to = from.Value.AddMonths(1);
                }
            }

            var items = await _repo.ListAsync(from, to, categoryId, type);
            return items.Select(m => new MovementListItemDto(
                m.Id, m.Type, m.Description, m.MovimentDate, m.IsRecurring,
                m.CategoryId, m.Category?.Name ?? string.Empty, m.Value)).ToList();
        }
    }
}
