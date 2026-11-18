using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Finance.Tests.Integration;

[Collection("Database")]
public class IsolationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public IsolationTests(ApiFactory factory) => _factory = factory;

    private static async Task<int> CreateCategoryAsync(HttpClient c, string name)
    {
        var r = await c.PostAsJsonAsync("/api/Category", new { name });
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task<int> CreateIncomeAsync(HttpClient c, int categoryId, decimal value = 10m)
    {
        var r = await c.PostAsJsonAsync("/api/Income", new
        {
            description = "salary", movimentDate = "2026-01-02", isRecurring = false, categoryId, value
        });
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private async Task<(HttpClient a, HttpClient b)> TwoUsersAsync()
    {
        await _factory.ResetDatabaseAsync();
        return (await _factory.CreateLoggedInClientAsync("a@example.com"),
                await _factory.CreateLoggedInClientAsync("b@example.com"));
    }

    [Fact]
    public async Task UserB_CannotGetUserA_Income()
    {
        var (a, b) = await TwoUsersAsync();
        var id = await CreateIncomeAsync(a, await CreateCategoryAsync(a, "Salary"));
        Assert.Equal(HttpStatusCode.OK, (await a.GetAsync($"/api/Income/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/Income/{id}")).StatusCode);
    }

    [Fact]
    public async Task UserB_CannotUpdateUserA_Income()
    {
        var (a, b) = await TwoUsersAsync();
        var catA = await CreateCategoryAsync(a, "Salary");
        var id = await CreateIncomeAsync(a, catA, 10m);
        var put = await b.PutAsJsonAsync($"/api/Income/{id}", new
        {
            description = "hacked", movimentDate = "2026-01-02", isRecurring = false, categoryId = catA, value = 999m
        });
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
        var still = await a.GetFromJsonAsync<JsonElement>($"/api/Income/{id}");
        Assert.Equal(10m, still.GetProperty("value").GetDecimal());
    }

    [Fact]
    public async Task UserB_CannotDeleteUserA_Income()
    {
        var (a, b) = await TwoUsersAsync();
        var id = await CreateIncomeAsync(a, await CreateCategoryAsync(a, "Salary"));
        Assert.Equal(HttpStatusCode.NotFound, (await b.DeleteAsync($"/api/Income/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await a.GetAsync($"/api/Income/{id}")).StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwnRecords()
    {
        var (a, b) = await TwoUsersAsync();
        var idA = await CreateIncomeAsync(a, await CreateCategoryAsync(a, "Salary"));
        var idB = await CreateIncomeAsync(b, await CreateCategoryAsync(b, "Salary"));
        var listA = await a.GetFromJsonAsync<JsonElement>("/api/Income");
        var idsA = listA.EnumerateArray().Select(e => e.GetProperty("id").GetInt32()).ToList();
        Assert.Contains(idA, idsA);
        Assert.DoesNotContain(idB, idsA);
    }

    [Fact]
    public async Task UserB_CannotSeeUserA_Categories()
    {
        var (a, b) = await TwoUsersAsync();
        var idA = await CreateCategoryAsync(a, "Private");
        var listA = await a.GetFromJsonAsync<JsonElement>("/api/Category");
        Assert.Contains(idA, listA.EnumerateArray().Select(e => e.GetProperty("id").GetInt32()));
        var listB = await b.GetFromJsonAsync<JsonElement>("/api/Category");
        Assert.DoesNotContain(idA, listB.EnumerateArray().Select(e => e.GetProperty("id").GetInt32()));
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/api/Category/{idA}")).StatusCode);
    }
}
