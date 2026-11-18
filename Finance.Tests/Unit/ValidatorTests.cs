using Finance.Application.Validators;
using Finance.Application.ViewModel;

namespace Finance.Tests.Unit;

public class ValidatorTests
{
    private static IncomeCreateDto Income(decimal value) => new()
    {
        Description = "Salario",
        MovimentDate = new DateOnly(2026, 1, 15),
        IsRecurring = false,
        CategoryId = 1,
        Value = value
    };

    [Fact]
    public void IncomeValidator_RejectsZeroValue()
        => Assert.False(new IncomeCreateDtoValidator().Validate(Income(0m)).IsValid);

    [Fact]
    public void IncomeValidator_RejectsNegativeValue()
        => Assert.False(new IncomeCreateDtoValidator().Validate(Income(-5m)).IsValid);

    [Fact]
    public void IncomeValidator_RejectsThreeDecimalPlaces()
        => Assert.False(new IncomeCreateDtoValidator().Validate(Income(10.005m)).IsValid);

    [Fact]
    public void IncomeValidator_AcceptsTwoDecimalPlaces()
        => Assert.True(new IncomeCreateDtoValidator().Validate(Income(10.05m)).IsValid);

    [Fact]
    public void ExpenseValidator_RejectsThreeDecimalPlaces()
    {
        var dto = new ExpenseCreateDto
        {
            Description = "Luz",
            MovimentDate = new DateOnly(2026, 1, 15),
            CategoryId = 1,
            Value = 10.005m
        };
        Assert.False(new ExpenseCreateDtoValidator().Validate(dto).IsValid);
    }

    [Fact]
    public void UserValidator_RejectsPasswordShorterThan8()
    {
        var dto = new UserCreateDto { UserName = "ana", Email = "ana@example.com", Password = "1234567" };
        Assert.False(new UserCreateDtoValidator().Validate(dto).IsValid);
    }
}
