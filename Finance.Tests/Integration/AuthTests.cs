using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Finance.Application.Interfaces;
using Finance.Domain.Models;
using Finance.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Finance.Tests.Integration;

[Collection("Database")]
public class AuthTests : IClassFixture<ApiFactory>
{
    private static readonly string[] DefaultNames =
        { "Alimentação", "Moradia", "Transporte", "Saúde", "Lazer", "Salário", "Outros" };

    private readonly ApiFactory _factory;
    public AuthTests(ApiFactory factory) => _factory = factory;

    private static HttpClient NewClient(ApiFactory f) =>
        f.CreateCookieClient();

    private static Task<HttpResponseMessage> RegisterAsync(HttpClient c, string email, string password = ApiFactory.DefaultPassword, object? extra = null) =>
        c.PostAsJsonAsync("/api/Auth/register", extra ?? new { userName = "tester", email, password });

    private static async Task<string> CodeAsync(HttpResponseMessage r) =>
        (await r.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString()!;

    private async Task<T> WithDb<T>(Func<FinanceContext, Task<T>> work, ApiFactory? f = null)
    {
        using var scope = (f ?? _factory).Services.CreateScope();
        return await work(scope.ServiceProvider.GetRequiredService<FinanceContext>());
    }

    [Fact]
    public async Task Register_AlwaysCreatesRoleUser_EvenIfRoleSent()
    {
        await _factory.ResetDatabaseAsync();
        var res = await RegisterAsync(NewClient(_factory), "role@example.com",
            extra: new { userName = "x", email = "role@example.com", password = ApiFactory.DefaultPassword, role = "admin" });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("user", body.GetProperty("role").GetString());
        Assert.False(body.TryGetProperty("password", out _));
        var role = await WithDb(db => db.User.Where(u => u.Email == "role@example.com").Select(u => u.Role).SingleAsync());
        Assert.Equal(Finance.Domain.Models.Enums.EUserRole.user, role);
    }

    [Fact]
    public async Task Register_SeedsDefaultCategories()
    {
        await _factory.ResetDatabaseAsync();
        var c = await _factory.CreateLoggedInClientAsync("seed@example.com");
        var list = await c.GetFromJsonAsync<JsonElement>("/api/Category");
        var names = list.EnumerateArray().Select(e => e.GetProperty("name").GetString()!).OrderBy(n => n).ToList();
        Assert.Equal(DefaultNames.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task Register_DuplicateEmail_CaseInsensitive_Returns409EmailTaken()
    {
        await _factory.ResetDatabaseAsync();
        var c = NewClient(_factory);
        Assert.Equal(HttpStatusCode.Created, (await RegisterAsync(c, "dup@example.com")).StatusCode);
        var res = await RegisterAsync(c, "  DUP@Example.com ");
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
        Assert.Equal("EMAIL_TAKEN", await CodeAsync(res));
        Assert.Equal(1, await WithDb(db => db.User.CountAsync()));
    }

    [Fact]
    public async Task Register_ShortPassword_Returns400()
    {
        await _factory.ResetDatabaseAsync();
        var res = await RegisterAsync(NewClient(_factory), "short@example.com", "1234567");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("VALIDATION_FAILED", await CodeAsync(res));
    }

    [Fact]
    public async Task Login_EmptyBody_Returns400()
    {
        var c = NewClient(_factory);
        var res = await c.PostAsJsonAsync("/api/Auth/login", new { });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        Assert.Equal("VALIDATION_FAILED", await CodeAsync(res));
        var empty = await c.PostAsync("/api/Auth/login", new StringContent("", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401InvalidCredentials()
    {
        await _factory.ResetDatabaseAsync();
        var c = NewClient(_factory);
        await RegisterAsync(c, "wrong@example.com");
        var res = await c.PostAsJsonAsync("/api/Auth/login", new { email = "wrong@example.com", password = "bad-password" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Equal("INVALID_CREDENTIALS", await CodeAsync(res));
        var unknown = await c.PostAsJsonAsync("/api/Auth/login", new { email = "nobody@example.com", password = "bad-password" });
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal("INVALID_CREDENTIALS", await CodeAsync(unknown));
    }

    [Fact]
    public async Task Login_SetsCookieWithSecurityFlags()
    {
        await _factory.ResetDatabaseAsync();
        var c = NewClient(_factory);
        await RegisterAsync(c, "cookie@example.com");
        var res = await c.PostAsJsonAsync("/api/Auth/login", new { email = "COOKIE@example.com", password = ApiFactory.DefaultPassword });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var cookie = res.Headers.GetValues("Set-Cookie").Single(h => h.StartsWith("jwt="));
        var lower = cookie.ToLowerInvariant();
        Assert.Contains("httponly", lower);
        Assert.Contains("secure", lower);
        Assert.Contains("samesite=strict", lower);
        var expiresText = cookie.Split(';').Select(p => p.Trim()).Single(p => p.StartsWith("expires=", StringComparison.OrdinalIgnoreCase))[8..];
        var expires = DateTimeOffset.Parse(expiresText, System.Globalization.CultureInfo.InvariantCulture);
        var hours = (expires - DateTimeOffset.UtcNow).TotalHours;
        Assert.InRange(hours, 7.9, 8.1);
    }

    private HttpClient ClientWithJwtCookie(string token)
    {
        // Secure cookie: the test handler only sends it over https, so set it as a header instead.
        var c = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = false });
        c.DefaultRequestHeaders.Add("Cookie", $"jwt={token}");
        return c;
    }

    [Fact]
    public async Task Protected_WithTamperedCookie_Returns401()
    {
        var res = await ClientWithJwtCookie("abc.def.ghi").GetAsync("/api/Category");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        var res2 = await ClientWithJwtCookie("garbage").GetAsync("/api/Auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, res2.StatusCode);

        await _factory.ResetDatabaseAsync();
        var c = NewClient(_factory);
        await RegisterAsync(c, "tamper@example.com");
        var login = await c.PostAsJsonAsync("/api/Auth/login", new { email = "tamper@example.com", password = ApiFactory.DefaultPassword });
        var jwt = login.Headers.GetValues("Set-Cookie").Single(h => h.StartsWith("jwt=")).Split(';')[0][4..];
        var tampered = jwt[..^3] + (jwt.EndsWith("AAA") ? "BBB" : "AAA");
        Assert.Equal(HttpStatusCode.Unauthorized, (await ClientWithJwtCookie(tampered).GetAsync("/api/Auth/me")).StatusCode);
    }

    [Fact]
    public async Task Protected_WithExpiredToken_Returns401()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiFactory.TestJwtKey));
        var token = new JwtSecurityToken(
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
            notBefore: DateTime.UtcNow.AddHours(-3),
            expires: DateTime.UtcNow.AddHours(-2),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        Assert.Equal(HttpStatusCode.Unauthorized, (await ClientWithJwtCookie(jwt).GetAsync("/api/Category")).StatusCode);
    }

    [Fact]
    public async Task Me_ReturnsCurrentUser()
    {
        await _factory.ResetDatabaseAsync();
        var c = await _factory.CreateLoggedInClientAsync("me@example.com");
        var res = await c.GetAsync("/api/Auth/me");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("me@example.com", body.GetProperty("email").GetString());
        Assert.Equal("user", body.GetProperty("role").GetString());
        Assert.False(body.TryGetProperty("password", out _));
    }

    [Fact]
    public async Task Me_Anonymous_Returns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await NewClient(_factory).GetAsync("/api/Auth/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await NewClient(_factory).DeleteAsync("/api/Auth/me")).StatusCode);
    }

    [Fact]
    public async Task OldUserEndpoints_AreNotExposed()
    {
        var c = NewClient(_factory);
        var res = await c.PostAsJsonAsync("/api/Auth/create", new { userName = "x", email = "x@example.com", password = ApiFactory.DefaultPassword });
        Assert.NotEqual(HttpStatusCode.Created, res.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync("/api/User")).StatusCode);
    }

    private static async Task AddMovementsAsync(HttpClient c)
    {
        var cats = await c.GetFromJsonAsync<JsonElement>("/api/Category");
        var catId = cats.EnumerateArray().First().GetProperty("id").GetInt32();
        (await c.PostAsJsonAsync("/api/Income", new { description = "pay", movimentDate = "2026-01-02", isRecurring = false, categoryId = catId, value = 10m })).EnsureSuccessStatusCode();
        (await c.PostAsJsonAsync("/api/Expense", new { description = "food", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 5m })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteMe_DemoUser_Returns403AndCanStillLogIn()
    {
        await _factory.ResetDatabaseAsync();
        var demo = await _factory.CreateLoggedInClientAsync("Demo@FinanceController.app");
        var res = await demo.DeleteAsync("/api/Auth/me");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.Equal("DEMO_READ_ONLY", await CodeAsync(res));
        Assert.Equal(1, await WithDb(db => db.User.IgnoreQueryFilters().CountAsync(u => u.Email.ToLower() == "demo@financecontroller.app")));

        var login = await NewClient(_factory).PostAsJsonAsync("/api/Auth/login",
            new { email = "demo@financecontroller.app", password = ApiFactory.DefaultPassword });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task DeleteMe_RemovesOwnDataOnly()
    {
        await _factory.ResetDatabaseAsync();
        var a = await _factory.CreateLoggedInClientAsync("del-a@example.com");
        var b = await _factory.CreateLoggedInClientAsync("del-b@example.com");
        await AddMovementsAsync(a);
        await AddMovementsAsync(b);

        var res = await a.DeleteAsync("/api/Auth/me");
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
        Assert.Contains(res.Headers.GetValues("Set-Cookie"), h => h.StartsWith("jwt=") && h.Contains("1970"));

        var (users, cats, movs, bCats, bMovs) = await WithDb(async db => (
            await db.User.IgnoreQueryFilters().CountAsync(),
            await db.Categories.IgnoreQueryFilters().CountAsync(),
            await db.Moviments.IgnoreQueryFilters().CountAsync(),
            await db.Categories.IgnoreQueryFilters().Where(c => c.UserId == db.User.Single(u => u.Email == "del-b@example.com").Id).CountAsync(),
            await db.Moviments.IgnoreQueryFilters().Where(m => m.UserId == db.User.Single(u => u.Email == "del-b@example.com").Id).CountAsync()));
        Assert.Equal(1, users);
        Assert.Equal(7, cats);
        Assert.Equal(7, bCats);
        Assert.Equal(2, movs);
        Assert.Equal(2, bMovs);

        Assert.Equal(7, (await b.GetFromJsonAsync<JsonElement>("/api/Category")).GetArrayLength());
        Assert.Equal(1, (await b.GetFromJsonAsync<JsonElement>("/api/Income")).GetArrayLength());

        var login = await NewClient(_factory).PostAsJsonAsync("/api/Auth/login", new { email = "del-a@example.com", password = ApiFactory.DefaultPassword });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        // the old token no longer maps to a user
        Assert.Equal(HttpStatusCode.Unauthorized, (await a.GetAsync("/api/Auth/me")).StatusCode);
    }

    [Fact]
    public async Task NormalUser_CanCreateCategoryExpenseAndIncome()
    {
        await _factory.ResetDatabaseAsync();
        var c = await _factory.CreateLoggedInClientAsync("normal@example.com");
        var cat = await c.PostAsJsonAsync("/api/Category", new { name = "Custom" });
        Assert.Equal(HttpStatusCode.Created, cat.StatusCode);
        var catId = (await cat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var exp = await c.PostAsJsonAsync("/api/Expense", new { description = "e", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 5m });
        Assert.Equal(HttpStatusCode.Created, exp.StatusCode);
        var inc = await c.PostAsJsonAsync("/api/Income", new { description = "i", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 5m });
        Assert.Equal(HttpStatusCode.Created, inc.StatusCode);
    }

    [Fact]
    public async Task Put_WithRouteId_Works()
    {
        await _factory.ResetDatabaseAsync();
        var c = await _factory.CreateLoggedInClientAsync("put@example.com");
        var cat = await (await c.PostAsJsonAsync("/api/Category", new { name = "Before" })).Content.ReadFromJsonAsync<JsonElement>();
        var catId = cat.GetProperty("id").GetInt32();
        var put = await c.PutAsJsonAsync($"/api/Category/{catId}", new { name = "After" });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal("After", (await c.GetFromJsonAsync<JsonElement>($"/api/Category/{catId}")).GetProperty("name").GetString());

        var inc = await (await c.PostAsJsonAsync("/api/Income", new { description = "i", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 5m })).Content.ReadFromJsonAsync<JsonElement>();
        var incId = inc.GetProperty("id").GetInt32();
        var putInc = await c.PutAsJsonAsync($"/api/Income/{incId}", new { description = "i2", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 7m });
        Assert.Equal(HttpStatusCode.OK, putInc.StatusCode);
        Assert.Equal(7m, (await c.GetFromJsonAsync<JsonElement>($"/api/Income/{incId}")).GetProperty("value").GetDecimal());

        var exp = await (await c.PostAsJsonAsync("/api/Expense", new { description = "e", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 5m })).Content.ReadFromJsonAsync<JsonElement>();
        var expId = exp.GetProperty("id").GetInt32();
        var putExp = await c.PutAsJsonAsync($"/api/Expense/{expId}", new { description = "e2", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 9m });
        Assert.Equal(HttpStatusCode.OK, putExp.StatusCode);
        Assert.Equal(9m, (await c.GetFromJsonAsync<JsonElement>($"/api/Expense/{expId}")).GetProperty("value").GetDecimal());
    }

    [Fact]
    public async Task Delete_WithRouteId_Returns204()
    {
        await _factory.ResetDatabaseAsync();
        var c = await _factory.CreateLoggedInClientAsync("delroute@example.com");
        var cat = await (await c.PostAsJsonAsync("/api/Category", new { name = "Temp" })).Content.ReadFromJsonAsync<JsonElement>();
        var catId = cat.GetProperty("id").GetInt32();
        var inc = await (await c.PostAsJsonAsync("/api/Income", new { description = "i", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 5m })).Content.ReadFromJsonAsync<JsonElement>();
        var exp = await (await c.PostAsJsonAsync("/api/Expense", new { description = "e", movimentDate = "2026-01-03", isRecurring = false, categoryId = catId, value = 5m })).Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.NoContent, (await c.DeleteAsync($"/api/Income/{inc.GetProperty("id").GetInt32()}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await c.DeleteAsync($"/api/Expense/{exp.GetProperty("id").GetInt32()}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await c.DeleteAsync($"/api/Category/{catId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync($"/api/Category/{catId}")).StatusCode);
    }

    [Fact]
    public async Task Login_SixthAttemptInAMinute_Returns429()
    {
        using var limited = new ApiFactory(new Dictionary<string, string?> { ["RateLimit__LoginPerMinute"] = null });
        var c = NewClient(limited);
        for (var i = 1; i <= 5; i++)
        {
            var r = await c.PostAsJsonAsync("/api/Auth/login", new { email = "rl@example.com", password = "whatever-1" });
            Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        }
        var sixth = await c.PostAsJsonAsync("/api/Auth/login", new { email = "rl@example.com", password = "whatever-1" });
        Assert.Equal(HttpStatusCode.TooManyRequests, sixth.StatusCode);
        var body = await sixth.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("RATE_LIMITED", body.GetProperty("code").GetString());
        Assert.True(body.TryGetProperty("message", out _));
    }

    private sealed class ThrowingCategoryRepository : ICategoryRepository
    {
        public Task AddAsync(Category entity) => throw new InvalidOperationException("seed failure");
        public Task DeleteAsync(int id) => throw new NotSupportedException();
        public Task<IEnumerable<Category>> GetAllAsync() => throw new NotSupportedException();
        public Task<Category?> GetByIdAsync(int id) => throw new NotSupportedException();
        public Task UpdateAsync(int id, Category entity) => throw new NotSupportedException();
    }

    [Fact]
    public async Task Register_IsAtomic_NoUserWhenCategorySeedFails()
    {
        await _factory.ResetDatabaseAsync();
        using var broken = _factory.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s => s.AddScoped<ICategoryRepository, ThrowingCategoryRepository>()));
        var fail = await RegisterAsync(broken.CreateClient(), "atomic@example.com");
        Assert.Equal(HttpStatusCode.InternalServerError, fail.StatusCode);
        Assert.Equal(0, await WithDb(db => db.User.CountAsync(u => u.Email == "atomic@example.com")));
    }
}
