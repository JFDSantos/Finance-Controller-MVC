using Finance.Application.ViewModel;
using Finance.Domain.Models.Enums;

namespace Finance.Application.Interfaces
{
    public interface IMovementQueryService
    {
        Task<IReadOnlyList<MovementListItemDto>> ListAsync(int? year, int? month, int? categoryId, EMovimentType? type);
    }
}
