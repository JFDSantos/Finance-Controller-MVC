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
}
