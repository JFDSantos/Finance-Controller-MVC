namespace Finance.Application.ViewModel
{
    public record MonthlySummaryDto(
        int Year, int Month, decimal Income, decimal Expense, decimal Balance,
        IReadOnlyList<CategoryTotalDto> ByCategory);
}
