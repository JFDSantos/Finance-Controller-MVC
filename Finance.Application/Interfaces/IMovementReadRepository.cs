using Finance.Application.ViewModel;
using Finance.Domain.Models;
using Finance.Domain.Models.Enums;

namespace Finance.Application.Interfaces
{
    public interface IMovementReadRepository
    {
        Task<bool> AnyByCategoryAsync(int categoryId);

        /// <summary>Ranges are [from, toExclusive). Ordered by date desc, then id desc. Includes Category.</summary>
        Task<IReadOnlyList<Moviment>> ListAsync(DateOnly? from, DateOnly? toExclusive, int? categoryId, EMovimentType? type);

        Task<IReadOnlyList<CategoryTotalDto>> TotalsByCategoryAsync(DateOnly from, DateOnly toExclusive);

        Task<IReadOnlyList<MonthPointDto>> TotalsByMonthAsync(DateOnly from, DateOnly toExclusive);
    }
}
