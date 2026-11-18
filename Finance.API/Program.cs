using Finance.API.Middleware;
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(
    cfg => {
        cfg.AddProfile<FinanceMappingProfile>();
    }, 
    typeof(FinanceMappingProfile).Assembly
);

builder.Services.AddControllers();

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
    // The hosting proxy is trusted.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Must be first so scheme/IP are corrected before HTTPS redirection and everything else.
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequireJsonMiddleware>();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
