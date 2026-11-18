using Finance.API.Health;
using Finance.API.Middleware;
using Finance.Infrastructure.Seed;
using Microsoft.AspNetCore.Mvc;
using Finance.Application.Interfaces;
using Finance.Application.Mappings;
using Finance.Application.Services;
using Finance.Application.Validators;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using Finance.Infrastructure.Data;
using Finance.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// The signing key comes only from configuration (env var Jwt__Key); no fallback.
builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= JwtOptions.MinKeyLength,
        $"Jwt:Key must be set (env var Jwt__Key) and have at least {JwtOptions.MinKeyLength} characters.")
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

// Registered after AddJwtBearer so it runs after TokenValidationParameters is assigned.
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwt) =>
    {
        options.TokenValidationParameters.IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Value.Key));
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Finance.Application.Interfaces.ICurrentUser, Finance.API.Auth.HttpCurrentUser>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FinanceContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // Tolerates a cold-starting / briefly unavailable database. Connection Timeout stays in the connection string.
        npgsql => npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

// AutoMapper
builder.Services.AddAutoMapper(
    cfg => {
        cfg.AddProfile<FinanceMappingProfile>();
    }, 
    typeof(FinanceMappingProfile).Assembly
);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Binding failures (missing required fields, malformed JSON, bad query types) use the same contract as FluentValidation.
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var errors = ctx.ModelState
                .Where(kv => kv.Value is { Errors.Count: > 0 })
                .SelectMany(kv => kv.Value!.Errors.Select(e => new
                {
                    propertyName = kv.Key.TrimStart('$', '.'),
                    // Never echo parser internals for body errors (e.g. line/byte positions).
                    errorMessage = string.IsNullOrEmpty(kv.Key) || kv.Key.StartsWith('$')
                        ? "The request body is missing or invalid."
                        : (string.IsNullOrWhiteSpace(e.ErrorMessage) ? "The value is invalid." : e.ErrorMessage),
                    errorCode = "INVALID_VALUE"
                }))
                .ToList();
            if (errors.Count == 0)
                errors.Add(new { propertyName = "", errorMessage = "The request is invalid.", errorCode = "INVALID_VALUE" });
            return new BadRequestObjectResult(new { code = "VALIDATION_FAILED", message = "Validation failed", errors })
            { ContentTypes = { "application/json" } };
        };
    });

builder.Services.AddScoped<IBaseRepository<Category, int>, CategoryRepository>();
builder.Services.AddScoped<IBaseRepository<Expense, int>, ExpenseRepository>();
builder.Services.AddScoped<IBaseRepository<Income, int>, IncomeRepository>();
builder.Services.AddScoped<IBaseRepository<User, int>, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<Finance.Application.Interfaces.IMovementReadRepository, MovementReadRepository>();


var services = builder.Services;

// Generic Services (for extensibility)
services.AddScoped<IBaseService<Expense,ExpenseCreateDto,ExpenseSelectDto>, ExpenseService>(); 
services.AddScoped<IBaseService<Income,IncomeCreateDto,IncomeSelectDto>, IncomeService>();
services.AddScoped<IBaseService<Category,CategoryCreateDto,CategorySelectDto>, CategoryService>();
services.AddScoped<IBaseService<User,UserCreateDto,UserSelectDto>, UserService>();

// Specific Services (for backward compatibility with Controllers)
services.AddScoped<IExpenseService, ExpenseService>();
services.AddScoped<IIncomeService, IncomeService>();
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<IUserService, UserService>();

// Specific Repositories (for backward compatibility)
services.AddScoped<ICategoryRepository>(sp => sp.GetRequiredService<IBaseRepository<Category, int>>() as ICategoryRepository ?? throw new InvalidOperationException());
services.AddScoped<IExpenseRepository>(sp => sp.GetRequiredService<IBaseRepository<Expense, int>>() as IExpenseRepository ?? throw new InvalidOperationException());
services.AddScoped<IIncomeRepository>(sp => sp.GetRequiredService<IBaseRepository<Income, int>>() as IIncomeRepository ?? throw new InvalidOperationException());
services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<IBaseRepository<User, int>>() as IUserRepository ?? throw new InvalidOperationException());

services.AddScoped<IJWTService, JWTService>();
services.AddSingleton(TimeProvider.System);
services.AddScoped<ISummaryService, SummaryService>();
services.AddScoped<IMovementQueryService, MovementQueryService>();

// FluentValidation
services.AddScoped<IValidator<ExpenseCreateDto>, ExpenseCreateDtoValidator>();
services.AddScoped<IValidator<IncomeCreateDto>, IncomeCreateDtoValidator>();
services.AddScoped<IValidator<CategoryCreateDto>, CategoryCreateDtoValidator>();
services.AddScoped<IValidator<UserCreateDto>, UserCreateDtoValidator>();
services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();

// Rate limiting: "login" policy, fixed window per client IP. The limit is read lazily from configuration.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { code = "RATE_LIMITED", message = "Too many requests. Try again in a minute." }, token);
    };
});
builder.Services.AddOptions<RateLimiterOptions>().Configure<IConfiguration>((options, config) =>
{
    var permitLimit = config.GetValue<int?>("RateLimit:LoginPerMinute") ?? 5;
    var registerPerHour = config.GetValue<int?>("RateLimit:RegisterPerHour") ?? 10;
    options.AddPolicy("register", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = registerPerHour,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0
            }));
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Trust assumption: the app is reachable ONLY through the hosting proxy, which sets X-Forwarded-For/Proto,
    // so every proxy is trusted. If the app were exposed directly, X-Forwarded-For would be spoofable and a
    // client could dodge the per-IP rate limits by forging its address.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Must be first so scheme/IP are corrected before HTTPS redirection and everything else.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// React build (copied to wwwroot at deploy time).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseMiddleware<RequireJsonMiddleware>();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealth();
app.MapControllers();

// SPA fallback: index.html for client-side routes, JSON 404 for anything under /api or /health.
app.MapFallback("{*path}", async (HttpContext ctx, IWebHostEnvironment env) =>
{
    var path = ctx.Request.Path;
    var index = env.WebRootFileProvider.GetFileInfo("index.html");
    var isApiOrHealth = path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
    if (isApiOrHealth || !index.Exists || !HttpMethods.IsGet(ctx.Request.Method))
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsJsonAsync(new { code = "NOT_FOUND", message = "Resource not found" });
        return;
    }
    ctx.Response.ContentType = "text/html; charset=utf-8";
    await ctx.Response.SendFileAsync(index);
});

// Optional demo account. A cold or unreachable database must never prevent startup.
if (app.Configuration.GetValue<bool>("Demo:Enabled") && !string.IsNullOrWhiteSpace(app.Configuration["Demo:Password"]))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<DbContextOptions<FinanceContext>>();
        await using var seedContext = new FinanceContext(options, new StartupNoUser());
        await DemoSeeder.SeedAsync(seedContext, app.Configuration["Demo:Password"]!);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Demo account seeding failed; continuing startup");
    }
}

app.Run();

public partial class Program { }

/// <summary>No HTTP user at startup; throws if anything reads it, so a query filter can never silently apply to user 0.</summary>
internal sealed class StartupNoUser : ICurrentUser
{
    public int UserId => throw new InvalidOperationException("No current user at startup.");
}
