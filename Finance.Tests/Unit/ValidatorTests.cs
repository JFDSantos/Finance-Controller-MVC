using Moq;
using Finance.Application.Validators;
using Finance.Application.ViewModel;

namespace Finance.Tests.Unit;

public class ValidatorTests
{
    private static Finance.Application.Interfaces.ICategoryRepository Repo()
    {
        var m = new Mock<Finance.Application.Interfaces.ICategoryRepository>();
        m.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Finance.Domain.Models.Category { Id = 1, Name = "c" });
        return m.Object;
    }

    private static IncomeCreateDto Income(decimal value) => new()
    {
        Description = "Salario",
        MovimentDate = new DateOnly(2026, 1, 15),
        IsRecurring = false,
        CategoryId = 1,
        Value = value
    };

    [Fact]
    public async Task IncomeValidator_RejectsZeroValue()
        => Assert.False((await new IncomeCreateDtoValidator(Repo()).ValidateAsync(Income(0m))).IsValid);

    [Fact]
    public async Task IncomeValidator_RejectsNegativeValue()
        => Assert.False((await new IncomeCreateDtoValidator(Repo()).ValidateAsync(Income(-5m))).IsValid);

    [Fact]
    public async Task IncomeValidator_RejectsThreeDecimalPlaces()
        => Assert.False((await new IncomeCreateDtoValidator(Repo()).ValidateAsync(Income(10.005m))).IsValid);

    [Fact]
    public async Task IncomeValidator_AcceptsTwoDecimalPlaces()
        => Assert.True((await new IncomeCreateDtoValidator(Repo()).ValidateAsync(Income(10.05m))).IsValid);

    [Fact]
    public async Task ExpenseValidator_RejectsThreeDecimalPlaces()
    {
        var dto = new ExpenseCreateDto
        {
            Description = "Luz",
            MovimentDate = new DateOnly(2026, 1, 15),
            CategoryId = 1,
            Value = 10.005m
        };
        Assert.False((await new ExpenseCreateDtoValidator(Repo()).ValidateAsync(dto)).IsValid);
    }

    [Fact]
    public void UserValidator_RejectsPasswordShorterThan8()
    {
        var dto = new UserCreateDto { UserName = "ana", Email = "ana@example.com", Password = "1234567" };
        Assert.False(new UserCreateDtoValidator().Validate(dto).IsValid);
    }

    private static IncomeCreateDto IncomeOn(DateOnly d) => new()
    {
        Description = "Salario", MovimentDate = d, IsRecurring = false, CategoryId = 1, Value = 10m
    };

    private static ExpenseCreateDto ExpenseOn(DateOnly d) => new()
    {
        Description = "Luz", MovimentDate = d, CategoryId = 1, Value = 10m
    };

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(1999, 12, 31)]
    [InlineData(2101, 1, 1)]
    public async Task MovementValidators_RejectDatesOutsideSupportedYears(int y, int m, int d)
    {
        var date = new DateOnly(y, m, d);
        var income = await new IncomeCreateDtoValidator(Repo()).ValidateAsync(IncomeOn(date));
        var expense = await new ExpenseCreateDtoValidator(Repo()).ValidateAsync(ExpenseOn(date));
        Assert.Contains(income.Errors, e => e.ErrorCode == "INVALID_DATE");
        Assert.Contains(expense.Errors, e => e.ErrorCode == "INVALID_DATE");
    }

    [Theory]
    [InlineData(2000, 1, 1)]
    [InlineData(2100, 12, 31)]
    public async Task MovementValidators_AcceptEdgeYears(int y, int m, int d)
    {
        var date = new DateOnly(y, m, d);
        Assert.True((await new IncomeCreateDtoValidator(Repo()).ValidateAsync(IncomeOn(date))).IsValid);
        Assert.True((await new ExpenseCreateDtoValidator(Repo()).ValidateAsync(ExpenseOn(date))).IsValid);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("a@")]
    public void UserValidator_RejectsInvalidEmail(string email)
    {
        var dto = new UserCreateDto { UserName = "ana", Email = email, Password = "12345678" };
        Assert.Contains(new UserCreateDtoValidator().Validate(dto).Errors, e => e.PropertyName == "Email");
    }
}
