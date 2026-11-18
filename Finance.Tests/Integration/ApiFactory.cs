using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Finance.Infrastructure.Data;

namespace Finance.Tests.Integration;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string TestJwtKey = "test-only-jwt-key-0123456789-abcdefghij"; // 40 chars
    public const string DefaultPassword = "Senha#12345";

    private readonly Dictionary<string, string?> _overrides;

    /// <param name="configOverrides">Extra/overriding configuration; a null value removes the key (e.g. ["Jwt__Key"] = null).</param>
    public ApiFactory(IDictionary<string, string?>? configOverrides = null)
    {
        _overrides = new Dictionary<string, string?>
        {
            ["Jwt__Key"] = TestJwtKey,
            ["ConnectionStrings__DefaultConnection"] =
                Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
                ?? "Host=localhost;Database=finance_test;Username=postgres;Password=postgres",
        };
        if (configOverrides != null)
            foreach (var kv in configOverrides) _overrides[kv.Key] = kv.Value;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Normalize "A__B" to "A:B" so overrides win over appsettings.
            var map = _overrides.ToDictionary(kv => kv.Key.Replace("__", ":"), kv => kv.Value);
            config.AddInMemoryCollection(map);
        });
    }

    public async Task<HttpClient> CreateLoggedInClientAsync(string email, string password = DefaultPassword)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        // Register may return 4xx if the user already exists; login decides.
        await client.PostAsJsonAsync("/api/Auth/register", new { email, password });
        var login = await client.PostAsJsonAsync("/api/Auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }
}
