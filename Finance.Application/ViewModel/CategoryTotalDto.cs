using Finance.Domain.Models.Enums;

namespace Finance.Application.ViewModel
{
    public record CategoryTotalDto(int CategoryId, string CategoryName, EMovimentType Type, decimal Total);
}
