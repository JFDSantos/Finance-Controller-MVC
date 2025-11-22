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
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


var services = builder.Services;

services.AddScoped<IExpenseService, ExpenseService>(); 
services.AddScoped<IIncomeService, IncomeService>();
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<IUserService, UserService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
