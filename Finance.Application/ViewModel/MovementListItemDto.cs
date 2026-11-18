using Finance.Domain.Models.Enums;

namespace Finance.Application.ViewModel
{
    public record MovementListItemDto(
        int Id, EMovimentType Type, string? Description, DateOnly MovimentDate, bool IsRecurring,
        int CategoryId, string CategoryName, decimal Value);
}
