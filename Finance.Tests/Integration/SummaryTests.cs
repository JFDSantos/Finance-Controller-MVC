using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Finance.Tests.Integration;

[Collection("Database")]
public class SummaryTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public SummaryTests(ApiFactory factory) => _factory = factory;

    private async Task<HttpClient> UserAsync(string email = "a@example.com")
    {
        await _factory.ResetDatabaseAsync();
        return await _factory.CreateLoggedInClientAsync(email);
    }

    private static async Task<int> FirstCategoryIdAsync(HttpClient c, int skip = 0)
    {
        var list = await c.GetFromJsonAsync<JsonElement>("/api/Category");
        return list.EnumerateArray().Skip(skip).First().GetProperty("id").GetInt32();
    }

    private static async Task PostAsync(HttpClient c, string kind, string date, decimal value, int categoryId)
    {
        var r = await c.PostAsJsonAsync($"/api/{kind}", new
        {
            description = "x", movimentDate = date, isRecurring = false, categoryId, value
        });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    }

    private static Task<JsonElement> SummaryAsync(HttpClient c, int year, int month)
        => c.GetFromJsonAsync<JsonElement>($"/api/Summary?year={year}&month={month}");

    [Fact]
    public async Task Summary_EmptyMonth_ReturnsZerosAndEmptyByCategory()
    {
        var c = await UserAsync();
        var s = await SummaryAsync(c, 2026, 3);
        Assert.Equal(2026, s.GetProperty("year").GetInt32());
        Assert.Equal(3, s.GetProperty("month").GetInt32());
        Assert.Equal(0m, s.GetProperty("income").GetDecimal());
        Assert.Equal(0m, s.GetProperty("expense").GetDecimal());
        Assert.Equal(0m, s.GetProperty("balance").GetDecimal());
        Assert.Equal(0, s.GetProperty("byCategory").GetArrayLength());
    }

    [Fact]
    public async Task Summary_BoundaryDates_Jan31_Feb1_Dec31_Jan1_LandInCorrectMonth()
    {
        var c = await UserAsync();
        var cat = await FirstCategoryIdAsync(c);
        await PostAsync(c, "Income", "2026-01-31", 1m, cat);
        await PostAsync(c, "Income", "2026-02-01", 10m, cat);
        await PostAsync(c, "Income", "2026-12-31", 100m, cat);
        await PostAsync(c, "Income", "2027-01-01", 1000m, cat);

        Assert.Equal(1m, (await SummaryAsync(c, 2026, 1)).GetProperty("income").GetDecimal());
        Assert.Equal(10m, (await SummaryAsync(c, 2026, 2)).GetProperty("income").GetDecimal());
        Assert.Equal(100m, (await SummaryAsync(c, 2026, 12)).GetProperty("income").GetDecimal());
        Assert.Equal(1000m, (await SummaryAsync(c, 2027, 1)).GetProperty("income").GetDecimal());
    }

    [Fact]
    public async Task Summary_LeapDay_2028_02_29_CountedInFebruary()
    {
        var c = await UserAsync();
        var cat = await FirstCategoryIdAsync(c);
        await PostAsync(c, "Expense", "2028-02-29", 25.5m, cat);
        await PostAsync(c, "Expense", "2028-03-01", 7m, cat);

        var feb = await SummaryAsync(c, 2028, 2);
        Assert.Equal(25.5m, feb.GetProperty("expense").GetDecimal());
        Assert.Equal(7m, (await SummaryAsync(c, 2028, 3)).GetProperty("expense").GetDecimal());
    }

    [Fact]
    public async Task Summary_Balance_IsIncomeMinusExpense()
    {
        var c = await UserAsync();
        var cat1 = await FirstCategoryIdAsync(c);
        var cat2 = await FirstCategoryIdAsync(c, 1);
        await PostAsync(c, "Income", "2026-05-10", 1000.75m, cat1);
        await PostAsync(c, "Income", "2026-05-11", 200m, cat1);
        await PostAsync(c, "Expense", "2026-05-12", 300.25m, cat2);
        await PostAsync(c, "Expense", "2026-05-13", 50m, cat2);

        var s = await SummaryAsync(c, 2026, 5);
        Assert.Equal(1200.75m, s.GetProperty("income").GetDecimal());
        Assert.Equal(350.25m, s.GetProperty("expense").GetDecimal());
        Assert.Equal(850.50m, s.GetProperty("balance").GetDecimal());

        var by = s.GetProperty("byCategory").EnumerateArray().ToList();
        Assert.Equal(2, by.Count);
        var inc = by.Single(e => e.GetProperty("type").GetString() == "Income");
        Assert.Equal(cat1, inc.GetProperty("categoryId").GetInt32());
        Assert.Equal(1200.75m, inc.GetProperty("total").GetDecimal());
        Assert.False(string.IsNullOrEmpty(inc.GetProperty("categoryName").GetString()));
        var exp = by.Single(e => e.GetProperty("type").GetString() == "Expense");
        Assert.Equal(350.25m, exp.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Summary_OnlyCountsOwnData()
    {
        var a = await UserAsync("a@example.com");
        var b = await _factory.CreateLoggedInClientAsync("b@example.com");
        await PostAsync(a, "Income", "2026-06-10", 500m, await FirstCategoryIdAsync(a));
        await PostAsync(b, "Income", "2026-06-10", 7m, await FirstCategoryIdAsync(b));

        Assert.Equal(500m, (await SummaryAsync(a, 2026, 6)).GetProperty("income").GetDecimal());
        Assert.Equal(7m, (await SummaryAsync(b, 2026, 6)).GetProperty("income").GetDecimal());
        var listB = await b.GetFromJsonAsync<JsonElement>("/api/Movement?year=2026&month=6");
        Assert.Equal(1, listB.GetArrayLength());
    }

    [Fact]
    public async Task Summary_MissingParams_Returns400WithCode()
    {
        var c = await UserAsync();
        foreach (var url in new[] { "/api/Summary", "/api/Summary?year=2026", "/api/Summary?month=5" })
        {
            var r = await c.GetAsync(url);
            Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
            var body = await r.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("VALIDATION_FAILED", body.GetProperty("code").GetString());
        }
        var bad = await c.GetAsync("/api/Summary?year=2026&month=13");
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        var err = (await bad.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("errors")[0];
        Assert.Equal("INVALID_MONTH", err.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Series_ReturnsTwelveMonthsZeroFilledInOrder()
    {
        var c = await UserAsync();
        var now = DateTime.UtcNow;
        var cur = new DateOnly(now.Year, now.Month, 1);
        await PostAsync(c, "Income", cur.ToString("yyyy-MM-dd"), 42m, await FirstCategoryIdAsync(c));
        // 12 months back (outside the window) must not appear
        await PostAsync(c, "Income", cur.AddMonths(-12).ToString("yyyy-MM-dd"), 999m, await FirstCategoryIdAsync(c));

        var arr = (await c.GetFromJsonAsync<JsonElement>("/api/Summary/series")).EnumerateArray().ToList();

        Assert.Equal(12, arr.Count);
        var first = cur.AddMonths(-11);
        Assert.Equal(first.Year, arr[0].GetProperty("year").GetInt32());
        Assert.Equal(first.Month, arr[0].GetProperty("month").GetInt32());
        Assert.Equal(cur.Year, arr[11].GetProperty("year").GetInt32());
        Assert.Equal(cur.Month, arr[11].GetProperty("month").GetInt32());
        for (var i = 0; i < 12; i++)
        {
            var expected = first.AddMonths(i);
            Assert.Equal(expected.Year, arr[i].GetProperty("year").GetInt32());
            Assert.Equal(expected.Month, arr[i].GetProperty("month").GetInt32());
        }
        Assert.Equal(42m, arr[11].GetProperty("income").GetDecimal());
        for (var i = 0; i < 11; i++)
        {
            Assert.Equal(0m, arr[i].GetProperty("income").GetDecimal());
            Assert.Equal(0m, arr[i].GetProperty("expense").GetDecimal());
        }

        var three = (await c.GetFromJsonAsync<JsonElement>("/api/Summary/series?months=3")).GetArrayLength();
        Assert.Equal(3, three);
    }

    [Theory]
    [InlineData(25)]
    [InlineData(0)]
    public async Task Series_MonthsAbove24_Returns400(int months)
    {
        var c = await UserAsync();
        var r = await c.GetAsync($"/api/Summary/series?months={months}");
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        var body = await r.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_FAILED", body.GetProperty("code").GetString());
        Assert.Equal("INVALID_RANGE", body.GetProperty("errors")[0].GetProperty("errorCode").GetString());
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/api/Summary/series?months=24")).StatusCode);
    }

    [Fact]
    public async Task Movement_FilterByCategoryAndMonth()
    {
        var c = await UserAsync();
        var cat1 = await FirstCategoryIdAsync(c);
        var cat2 = await FirstCategoryIdAsync(c, 1);
        await PostAsync(c, "Expense", "2026-04-01", 1m, cat1);
        await PostAsync(c, "Expense", "2026-04-30", 2m, cat1);
        await PostAsync(c, "Expense", "2026-04-15", 3m, cat2);
        await PostAsync(c, "Expense", "2026-05-01", 4m, cat1);
        await PostAsync(c, "Expense", "2026-03-31", 5m, cat1);

        var list = (await c.GetFromJsonAsync<JsonElement>($"/api/Movement?year=2026&month=4&categoryId={cat1}"))
            .EnumerateArray().ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { "2026-04-30", "2026-04-01" },
            list.Select(e => e.GetProperty("movimentDate").GetString()).ToArray());
        Assert.All(list, e => Assert.Equal(cat1, e.GetProperty("categoryId").GetInt32()));
        Assert.Equal("Expense", list[0].GetProperty("type").GetString());
        Assert.False(string.IsNullOrEmpty(list[0].GetProperty("categoryName").GetString()));

        // year only -> whole year
        var year = await c.GetFromJsonAsync<JsonElement>("/api/Movement?year=2026");
        Assert.Equal(5, year.GetArrayLength());
        // no filter -> everything
        var all = await c.GetFromJsonAsync<JsonElement>("/api/Movement");
        Assert.Equal(5, all.GetArrayLength());
    }

    [Fact]
    public async Task Movement_FilterByType()
    {
        var c = await UserAsync();
        var cat = await FirstCategoryIdAsync(c);
        await PostAsync(c, "Income", "2026-04-01", 10m, cat);
        await PostAsync(c, "Expense", "2026-04-02", 20m, cat);
        await PostAsync(c, "Expense", "2026-04-03", 30m, cat);

        var inc = await c.GetFromJsonAsync<JsonElement>("/api/Movement?type=Income");
        Assert.Equal(1, inc.GetArrayLength());
        Assert.Equal("Income", inc[0].GetProperty("type").GetString());
        var exp = await c.GetFromJsonAsync<JsonElement>("/api/Movement?type=expense");
        Assert.Equal(2, exp.GetArrayLength());
        Assert.All(exp.EnumerateArray(), e => Assert.Equal("Expense", e.GetProperty("type").GetString()));
    }

    [Theory]
    [InlineData("/api/Movement?year=2026&month=13", "INVALID_MONTH")]
    [InlineData("/api/Movement?year=2026&month=0", "INVALID_MONTH")]
    [InlineData("/api/Movement?month=5", "INVALID_YEAR")]
    [InlineData("/api/Movement?year=1999", "INVALID_YEAR")]
    public async Task Movement_InvalidMonth_Returns400(string url, string code)
    {
        var c = await UserAsync();
        await PostAsync(c, "Income", "2026-05-01", 1m, await FirstCategoryIdAsync(c));
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/api/Movement?year=2026&month=5")).StatusCode);
        var r = await c.GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        var body = await r.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("VALIDATION_FAILED", body.GetProperty("code").GetString());
        Assert.Equal(code, body.GetProperty("errors")[0].GetProperty("errorCode").GetString());
    }
}
