using Finance.Application.Interfaces;
using Finance.Application.Services;
using Finance.Application.ViewModel;
using Finance.Domain.Models.Enums;
using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Finance.Tests.Unit;

public class SummaryServiceTests
{
    private static SummaryService Build(Mock<IMovementReadRepository> repo, DateTimeOffset now)
        => new(repo.Object, new FakeTimeProvider(now));

    [Fact]
    public async Task Series_InJanuary_CrossesYearBoundary_AndZeroFills()
    {
        var repo = new Mock<IMovementReadRepository>();
        repo.Setup(r => r.TotalsByMonthAsync(new DateOnly(2026, 2, 1), new DateOnly(2027, 2, 1)))
            .ReturnsAsync(new List<MonthPointDto>
            {
                new(2026, 12, 100m, 40m),
                new(2027, 1, 5m, 0m),
            });
        var svc = Build(repo, new DateTimeOffset(2027, 1, 15, 10, 0, 0, TimeSpan.Zero));

        var series = await svc.GetSeriesAsync(12);

        Assert.Equal(12, series.Count);
        Assert.Equal((2026, 2), (series[0].Year, series[0].Month));
        Assert.Equal((2027, 1), (series[11].Year, series[11].Month));
        Assert.Equal((2026, 12), (series[10].Year, series[10].Month));
        Assert.Equal(100m, series[10].Income);
        Assert.Equal(40m, series[10].Expense);
        Assert.Equal(5m, series[11].Income);
        Assert.All(series.Take(10), p => Assert.Equal((0m, 0m), (p.Income, p.Expense)));
        var keys = series.Select(p => p.Year * 12 + p.Month).ToList();
        Assert.Equal(keys.OrderBy(k => k), keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    public async Task Series_OutOfRange_ThrowsInvalidRange(int months)
    {
        var svc = Build(new Mock<IMovementReadRepository>(), DateTimeOffset.UtcNow);
        var ex = await Assert.ThrowsAsync<ValidationException>(() => svc.GetSeriesAsync(months));
        Assert.Equal("INVALID_RANGE", ex.Errors.Single().ErrorCode);
    }

    [Theory]
    [InlineData(2026, 0, "INVALID_MONTH")]
    [InlineData(2026, 13, "INVALID_MONTH")]
    [InlineData(1999, 5, "INVALID_YEAR")]
    [InlineData(2101, 5, "INVALID_YEAR")]
    public async Task Monthly_InvalidInput_Throws(int year, int month, string code)
    {
        var svc = Build(new Mock<IMovementReadRepository>(), DateTimeOffset.UtcNow);
        var ex = await Assert.ThrowsAsync<ValidationException>(() => svc.GetMonthlyAsync(year, month));
        Assert.Equal(code, ex.Errors.Single().ErrorCode);
    }

    [Fact]
    public async Task Monthly_ComputesTotalsAndBalance_WithHalfOpenRange()
    {
        var repo = new Mock<IMovementReadRepository>();
        repo.Setup(r => r.TotalsByCategoryAsync(new DateOnly(2026, 12, 1), new DateOnly(2027, 1, 1)))
            .ReturnsAsync(new List<CategoryTotalDto>
            {
                new(1, "Salary", EMovimentType.Income, 1000m),
                new(2, "Food", EMovimentType.Expense, 300.50m),
                new(3, "Rent", EMovimentType.Expense, 400m),
            });
        var svc = Build(repo, DateTimeOffset.UtcNow);

        var s = await svc.GetMonthlyAsync(2026, 12);

        Assert.Equal(1000m, s.Income);
        Assert.Equal(700.50m, s.Expense);
        Assert.Equal(299.50m, s.Balance);
        Assert.Equal(3, s.ByCategory.Count);
    }
}
