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
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"] ?? "chave-super-secreta-123!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
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

builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FinanceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(
    cfg => {
        cfg.AddProfile<FinanceMappingProfile>();
    }, 
    AppDomain.CurrentDomain.GetAssemblies()
);

builder.Services.AddControllers();

builder.Services.AddScoped<IBaseRepository<Category, int>, CategoryRepository>();
builder.Services.AddScoped<IBaseRepository<Expense, int>, ExpenseRepository>();
builder.Services.AddScoped<IBaseRepository<Income, int>, IncomeRepository>();
builder.Services.AddScoped<IBaseRepository<User, int>, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
