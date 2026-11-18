using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Finance.Application.Interfaces;
using Finance.Domain.Models;
using Finance.Infrastructure.Data;
using Finance.Infrastructure.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finance.Tests.Integration;

[ApiController]
[Route("__test/boom")]
public class BoomController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new InvalidOperationException("SECRET-INTERNAL-DETAIL");
}

[Collection("Database")]
public class ProductionTests
{
    private const string DemoEmail = "demo@financecontroller.app";
    private const string DemoPass = "throwaway-Demo#123";

    private static async Task<JsonElement> Json(HttpResponseMessage r) => await r.Content.ReadFromJsonAsync<JsonElement>();

    [Fact]
    public async Task Health_Returns200WithoutDatabase()
    {
        await using var f = new ApiFactory(new Dictionary<string, string?>
        { ["ConnectionStrings__DefaultConnection"] = "Host=invalid-host-xyz;Database=nope;Timeout=1" });
        var r = await f.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("ok", (await Json(r)).GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("/api/nope")]
    [InlineData("/api/nope/deeper.json")]
    [InlineData("/health/x")]
    public async Task UnknownApiRoute_Returns404Json_NotIndexHtml(string path)
    {
        await using var f = new ApiFactory();
        var r = await f.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Contains("json", r.Content.Headers.ContentType?.MediaType);
        Assert.Equal("NOT_FOUND", (await Json(r)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task SpaFallback_ServesIndexHtml_ExceptForApi()
    {
        var dir = Path.Combine(Path.GetTempPath(), "fc-web-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "index.html"), "<html>SPA-INDEX</html>");
        try
        {
            await using var f = new ApiFactory();
            using var web = f.WithWebHostBuilder(b => b.UseWebRoot(dir));
            var client = web.CreateClient();
            var spa = await client.GetAsync("/some/spa/route");
            Assert.Equal(HttpStatusCode.OK, spa.StatusCode);
            Assert.Contains("SPA-INDEX", await spa.Content.ReadAsStringAsync());
            var root = await client.GetAsync("/");
            Assert.Contains("SPA-INDEX", await root.Content.ReadAsStringAsync());
            var api = await client.GetAsync("/api/nope");
            Assert.Equal(HttpStatusCode.NotFound, api.StatusCode);
            Assert.DoesNotContain("SPA-INDEX", await api.Content.ReadAsStringAsync());
        }
        finally { Directory.Delete(dir, true); }
    }

    [Fact]
    public async Task SpaFallback_WithoutIndexHtml_DoesNotThrow()
    {
        await using var f = new ApiFactory();
        var r = await f.CreateClient().GetAsync("/some/spa/route");
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Swagger_NotAvailableInProduction()
    {
        await using var f = new ApiFactory();
        using var prod = f.WithWebHostBuilder(b => b.UseEnvironment("Production"));
        var client = prod.CreateClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger/v1/swagger.json")).StatusCode);
    }

    [Fact]
    public async Task Error500_DoesNotLeakStackTrace()
    {
        await using var f = new ApiFactory();
        using var boom = f.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            s.AddControllers().ConfigureApplicationPartManager(m =>
                m.ApplicationParts.Add(new AssemblyPart(typeof(BoomController).Assembly)))));
        var r = await boom.CreateClient().GetAsync("/__test/boom");
        Assert.Equal(HttpStatusCode.InternalServerError, r.StatusCode);
        var body = await r.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SECRET-INTERNAL-DETAIL", body);
        Assert.DoesNotContain("   at ", body);
        Assert.Equal("{\"code\":\"INTERNAL_ERROR\",\"message\":\"Internal server error\"}", body);
    }

    // ---- validation contract ----
    private static async Task AssertValidation400(HttpResponseMessage r)
    {
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        var j = await Json(r);
        Assert.Equal("VALIDATION_FAILED", j.GetProperty("code").GetString());
        var errors = j.GetProperty("errors");
        Assert.True(errors.GetArrayLength() > 0);
        var e = errors[0];
        Assert.True(e.TryGetProperty("propertyName", out _));
        Assert.True(e.TryGetProperty("errorMessage", out _));
        Assert.Equal("INVALID_VALUE", e.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Register_MissingFields_Returns400WithCode()
    {
        await using var f = new ApiFactory();
        var r = await f.CreateClient().PostAsJsonAsync("/api/Auth/register", new { email = "a@b.com" });
        await AssertValidation400(r);
    }

    [Fact]
    public async Task MalformedJson_Returns400WithCode()
    {
        await using var f = new ApiFactory();
        var r = await f.CreateClient().PostAsync("/api/Auth/register",
            new StringContent("{ not json", Encoding.UTF8, "application/json"));
        await AssertValidation400(r);
        Assert.DoesNotContain("LineNumber", await r.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/api/Movement?type=foo")]
    [InlineData("/api/Movement?type=5")]
    [InlineData("/api/Movement?year=abc")]
    [InlineData("/api/Summary?year=abc&month=1")]
    [InlineData("/api/Summary/series?months=abc")]
    public async Task BadQuery_Returns400WithCode(string url)
    {
        await using var f = new ApiFactory();
        await f.ResetDatabaseAsync();
        var client = await f.CreateLoggedInClientAsync("q" + Guid.NewGuid().ToString("N")[..8] + "@example.com");
        await AssertValidation400(await client.GetAsync(url));
    }

    // ---- DB retry ----
    [Fact]
    public async Task Register_WorksWithRetryEnabled()
    {
        await using var f = new ApiFactory();
        await f.ResetDatabaseAsync();
        var r = await f.CreateClient().PostAsJsonAsync("/api/Auth/register",
            new { userName = "retry", email = "retry@example.com", password = ApiFactory.DefaultPassword });
        Assert.Equal(HttpStatusCode.Created, r.StatusCode);
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
        Assert.True(db.Database.CreateExecutionStrategy().RetriesOnFailure);
        Assert.Equal(1, await db.User.CountAsync(u => u.Email == "retry@example.com"));
    }

    // ---- register rate limit ----
    [Fact]
    public async Task Register_ElevenAttemptsInAnHour_Returns429()
    {
        await using var f = new ApiFactory(new Dictionary<string, string?> { ["RateLimit__RegisterPerHour"] = null });
        await f.ResetDatabaseAsync();
        var client = f.CreateClient();
        for (var i = 0; i < 10; i++)
        {
            var ok = await client.PostAsJsonAsync("/api/Auth/register",
                new { userName = "u" + i, email = $"rl{i}@example.com", password = ApiFactory.DefaultPassword });
            Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
        }
        var r = await client.PostAsJsonAsync("/api/Auth/register",
            new { userName = "u11", email = "rl11@example.com", password = ApiFactory.DefaultPassword });
        Assert.Equal(HttpStatusCode.TooManyRequests, r.StatusCode);
        Assert.Equal("RATE_LIMITED", (await Json(r)).GetProperty("code").GetString());
    }

    // ---- demo seeder ----
    private static async Task<(int users, int cats, int movs, int recurring)> Counts(ApiFactory f)
    {
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
        var u = await db.User.SingleAsync(x => x.Email == DemoEmail);
        return (await db.User.CountAsync(x => x.Email == DemoEmail),
            await db.Categories.IgnoreQueryFilters().CountAsync(c => c.UserId == u.Id),
            await db.Moviments.IgnoreQueryFilters().CountAsync(m => m.UserId == u.Id),
            await db.Moviments.IgnoreQueryFilters().CountAsync(m => m.UserId == u.Id && m.IsRecurring));
    }

    [Fact]
    public async Task DemoSeeder_IsIdempotent()
    {
        await using var f = new ApiFactory();
        await f.ResetDatabaseAsync();
        using (var scope = f.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
            await DemoSeeder.SeedAsync(db, DemoPass);
        }
        var first = await Counts(f);
        using (var scope = f.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
            await DemoSeeder.SeedAsync(db, DemoPass);
        }
        var second = await Counts(f);
        Assert.Equal(first, second);
        Assert.Equal(1, first.users);
        Assert.Equal(7, first.cats);
        Assert.True(first.movs >= 20);
        Assert.True(first.recurring >= 2);
    }

    [Fact]
    public async Task DemoSeeder_Disabled_ByDefault()
    {
        await using var f = new ApiFactory();
        await f.ResetDatabaseAsync();
        f.CreateClient();
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
        Assert.False(await db.User.AnyAsync(u => u.Email == DemoEmail));
    }

    [Fact]
    public async Task DemoSeeder_EnabledWithoutPassword_DoesNothing()
    {
        await using var plain = new ApiFactory();
        await plain.ResetDatabaseAsync();
        await using var f = new ApiFactory(new Dictionary<string, string?> { ["Demo__Enabled"] = "true" });
        f.CreateClient();
        using var scope = f.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
        Assert.False(await db.User.AnyAsync(u => u.Email == DemoEmail));
    }

    [Fact]
    public async Task DemoSeeder_CanLogIn()
    {
        await using (var plain = new ApiFactory()) await plain.ResetDatabaseAsync();
        await using var f = new ApiFactory(new Dictionary<string, string?>
        { ["Demo__Enabled"] = "true", ["Demo__Password"] = DemoPass });
        var client = f.CreateCookieClient();
        var login = await client.PostAsJsonAsync("/api/Auth/login", new { email = DemoEmail, password = DemoPass });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var now = DateTime.UtcNow;
        var s = await client.GetAsync($"/api/Summary?year={now.Year}&month={now.Month}");
        Assert.Equal(HttpStatusCode.OK, s.StatusCode);
    }
}
