using System.Text.Json;
using AutoMapper;
using Finance.API.Middleware;
using Finance.Application.Exceptions;
using Finance.Application.Interfaces;
using Finance.Application.Mappings;
using Finance.Application.Services;
using Finance.Application.Validators;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Finance.Tests.Unit;

public class CategoryRulesTests
{
    private static Mock<ICategoryRepository> Repo(Category? result)
    {
        var m = new Mock<ICategoryRepository>();
        m.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(result);
        return m;
    }

    private static IncomeCreateDto Income() => new()
    { Description = "Salario", MovimentDate = new DateOnly(2026, 1, 15), CategoryId = 1, Value = 10m };

    private static ExpenseCreateDto Expense() => new()
    { Description = "Luz", MovimentDate = new DateOnly(2026, 1, 15), CategoryId = 1, Value = 10m };

    [Fact]
    public async Task Validator_RejectsUnknownCategory()
    {
        var repo = Repo(null).Object;
        var inc = await new IncomeCreateDtoValidator(repo).ValidateAsync(Income());
        var exp = await new ExpenseCreateDtoValidator(repo).ValidateAsync(Expense());
        Assert.Contains(inc.Errors, e => e.ErrorCode == "CATEGORY_NOT_FOUND" && e.PropertyName == "CategoryId");
        Assert.Contains(exp.Errors, e => e.ErrorCode == "CATEGORY_NOT_FOUND" && e.PropertyName == "CategoryId");
    }

    [Fact]
    public async Task Validator_AcceptsOwnedCategory()
    {
        var repo = Repo(new Category { Id = 1, Name = "c" }).Object;
        Assert.True((await new IncomeCreateDtoValidator(repo).ValidateAsync(Income())).IsValid);
        Assert.True((await new ExpenseCreateDtoValidator(repo).ValidateAsync(Expense())).IsValid);
    }

    private static CategoryService NewService(bool inUse, Mock<IBaseRepository<Category, int>> repo, Mock<IUnitOfWork> uow)
    {
        var read = new Mock<IMovementReadRepository>();
        read.Setup(r => r.AnyByCategoryAsync(It.IsAny<int>())).ReturnsAsync(inUse);
        var mapper = new MapperConfiguration(c => c.AddProfile<FinanceMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
        return new CategoryService(repo.Object, uow.Object, mapper, new InlineValidator<CategoryCreateDto>(), read.Object);
    }

    [Fact]
    public async Task DeleteCategory_WithMovements_ThrowsConflictCategoryInUse()
    {
        var repo = new Mock<IBaseRepository<Category, int>>();
        repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Category { Id = 3, Name = "c" });
        var uow = new Mock<IUnitOfWork>();
        var ex = await Assert.ThrowsAsync<ConflictException>(() => NewService(true, repo, uow).DeleteAsync(3));
        Assert.Equal("CATEGORY_IN_USE", ex.Code);
        repo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteCategory_Empty_Succeeds()
    {
        var repo = new Mock<IBaseRepository<Category, int>>();
        repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Category { Id = 3, Name = "c" });
        var uow = new Mock<IUnitOfWork>();
        await NewService(false, repo, uow).DeleteAsync(3);
        repo.Verify(r => r.DeleteAsync(3), Times.Once);
        uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    private static async Task<(int status, JsonElement body, string raw)> Run(Exception ex)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var mw = new ExceptionHandlingMiddleware(_ => throw ex, NullLogger<ExceptionHandlingMiddleware>.Instance);
        await mw.InvokeAsync(ctx);
        ctx.Response.Body.Position = 0;
        var raw = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        return (ctx.Response.StatusCode, JsonDocument.Parse(raw).RootElement.Clone(), raw);
    }

    [Fact]
    public async Task Middleware_Maps_ConflictTo409WithCode()
    {
        var (status, body, _) = await Run(new ConflictException("CATEGORY_IN_USE", "Category has movements"));
        Assert.Equal(409, status);
        Assert.Equal("CATEGORY_IN_USE", body.GetProperty("code").GetString());
        Assert.Equal("Category has movements", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Middleware_Maps_NotFoundTo404()
    {
        var (status, body, _) = await Run(new KeyNotFoundException("Entity not found"));
        Assert.Equal(404, status);
        Assert.Equal("NOT_FOUND", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Middleware_Maps_ValidationTo400WithErrors()
    {
        var failure = new FluentValidation.Results.ValidationFailure("CategoryId", "bad") { ErrorCode = "CATEGORY_NOT_FOUND" };
        var (status, body, _) = await Run(new ValidationException(new[] { failure }));
        Assert.Equal(400, status);
        Assert.Equal("VALIDATION_FAILED", body.GetProperty("code").GetString());
        var err = body.GetProperty("errors")[0];
        Assert.Equal("CategoryId", err.GetProperty("propertyName").GetString());
        Assert.Equal("bad", err.GetProperty("errorMessage").GetString());
        Assert.Equal("CATEGORY_NOT_FOUND", err.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Middleware_Maps_UnauthorizedTo401()
    {
        var (status, body, _) = await Run(new UnauthorizedAccessException("nope"));
        Assert.Equal(401, status);
        Assert.Equal("UNAUTHORIZED", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Middleware_Maps_UnexpectedTo500WithoutDetail()
    {
        var (status, body, raw) = await Run(new InvalidOperationException("secret db detail"));
        Assert.Equal(500, status);
        Assert.Equal("INTERNAL_ERROR", body.GetProperty("code").GetString());
        Assert.Equal("Internal server error", body.GetProperty("message").GetString());
        Assert.DoesNotContain("secret", raw);
        Assert.DoesNotContain("InvalidOperationException", raw);
    }
}
