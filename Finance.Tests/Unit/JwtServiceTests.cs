using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Finance.Application.Services;
using Finance.Application.ViewModel;
using Finance.Domain.Models.Enums;
using Finance.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finance.Tests.Unit;

public class JwtServiceTests
{
    private static JWTService CreateService() =>
        new(Options.Create(new JwtOptions { Key = ApiFactory.TestJwtKey }));

    private static JwtSecurityToken Generate(int id)
    {
        var token = CreateService().GenerateToken(new UserSelectDto
        {
            Id = id,
            user = "ana",
            email = "ana@example.com",
            role = default(EUserRole),
        });
        return new JwtSecurityTokenHandler().ReadJwtToken(token);
    }

    [Fact]
    public void GenerateToken_IncludesNameIdentifierClaim()
    {
        var jwt = Generate(7);
        // The handler maps ClaimTypes.NameIdentifier to the short "nameid" claim type in the payload.
        var claim = jwt.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid");
        Assert.Equal("7", claim.Value);
    }

    [Fact]
    public void GenerateToken_ExpiresInEightHours()
    {
        var jwt = Generate(1);
        var remaining = jwt.ValidTo - DateTime.UtcNow;
        Assert.InRange(remaining, TimeSpan.FromHours(7.9), TimeSpan.FromHours(8.01));
    }

    [Fact]
    public async Task Startup_FailsWhenJwtKeyMissing()
    {
        await using var factory = new ApiFactory(new Dictionary<string, string?> { ["Jwt__Key"] = null });
        var ex = Record.Exception(() => factory.CreateClient());
        Assert.NotNull(ex);
        var inner = ex;
        while (inner is not null && inner is not OptionsValidationException) inner = inner.InnerException;
        Assert.True(inner is OptionsValidationException, $"expected OptionsValidationException, got: {ex}");
        Assert.Contains("Jwt:Key", inner!.Message);
    }

    [Fact]
    public async Task Startup_FailsWhenJwtKeyTooShort()
    {
        await using var factory = new ApiFactory(new Dictionary<string, string?> { ["Jwt__Key"] = "short" });
        var ex = Record.Exception(() => factory.CreateClient());
        Assert.NotNull(ex);
        Assert.Contains("Jwt:Key", ex!.ToString());
    }

    [Fact]
    public async Task Startup_SucceedsWithValidJwtKey()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        Assert.NotNull(client);
    }

    [Fact]
    public async Task JwtBearer_UsesConfiguredKeyToValidateTokens()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        var opts = factory.Services
            .GetRequiredService<IOptionsMonitor<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>>()
            .Get("Bearer");
        var key = Assert.IsType<Microsoft.IdentityModel.Tokens.SymmetricSecurityKey>(
            opts.TokenValidationParameters.IssuerSigningKey);
        Assert.Equal(System.Text.Encoding.UTF8.GetBytes(ApiFactory.TestJwtKey), key.Key);
    }
}
