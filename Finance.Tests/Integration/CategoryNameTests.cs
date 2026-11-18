using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Finance.Tests.Integration;

[Collection("Database")]
public class CategoryNameTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public CategoryNameTests(ApiFactory factory) => _factory = factory;

    private static async Task<int> Create(HttpClient c, string name)
    {
        var r = await c.PostAsJsonAsync("/api/Category", new { name });
        r.EnsureSuccessStatusCode();
        return (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task AssertConflict(HttpResponseMessage r)
    {
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        var body = await r.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CATEGORY_NAME_TAKEN", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Post_DuplicateCategory_Returns409WithCode()
    {
        var c = await _factory.CreateLoggedInClientAsync("dup-post@example.com");
        await Create(c, "Viagens");
        await AssertConflict(await c.PostAsJsonAsync("/api/Category", new { name = "Viagens" }));
        await AssertConflict(await c.PostAsJsonAsync("/api/Category", new { name = "viagens" }));
    }

    [Fact]
    public async Task Put_RenameToExisting_Returns409WithCode()
    {
        var c = await _factory.CreateLoggedInClientAsync("dup-put@example.com");
        await Create(c, "Viagens");
        var id = await Create(c, "Pets");
        await AssertConflict(await c.PutAsJsonAsync($"/api/Category/{id}", new { name = "Viagens" }));
    }

    [Fact]
    public async Task TwoUsers_CanHaveSameCategoryName()
    {
        var a = await _factory.CreateLoggedInClientAsync("same-a@example.com");
        var b = await _factory.CreateLoggedInClientAsync("same-b@example.com");
        await Create(a, "Viagens");
        await Create(b, "Viagens");
    }
}
