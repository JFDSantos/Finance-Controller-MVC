using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface ISummaryService
    {
        Task<MonthlySummaryDto> GetMonthlyAsync(int year, int month);
        Task<IReadOnlyList<MonthPointDto>> GetSeriesAsync(int months);
    }
}
