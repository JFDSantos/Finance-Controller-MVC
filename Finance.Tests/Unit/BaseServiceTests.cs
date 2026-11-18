using AutoMapper;
using Finance.Application.Interfaces;
using Finance.Application.Mappings;
using Finance.Application.Services;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using Finance.Domain.Models.Enums;
using FluentValidation;
using Moq;

namespace Finance.Tests.Unit;

public class BaseServiceTests
{
    private static IMapper Mapper() =>
        new MapperConfiguration(c => c.AddProfile<FinanceMappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();

    private static IncomeService NewIncomeService(Mock<IBaseRepository<Income, int>> repo) =>
        new(repo.Object, Mock.Of<IUnitOfWork>(), Mapper(), new InlineValidator<IncomeCreateDto>());

    [Fact]
    public async Task UpdateAsync_PreservesIdAndUserId()
    {
        var existing = new Income { Id = 5, UserId = 2, CategoryId = 1, Value = 1m, Description = "old" };
        var repo = new Mock<IBaseRepository<Income, int>>();
        repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);
        var service = NewIncomeService(repo);

        var result = await service.UpdateAsync(5, new IncomeCreateDto
        {
            Description = "new", Value = 99.5m, CategoryId = 3, MovimentDate = new DateOnly(2026, 3, 4)
        });

        Assert.Equal(5, existing.Id);
        Assert.Equal(2, existing.UserId);
        Assert.Equal("new", existing.Description);
        Assert.Equal(99.5m, existing.Value);
        Assert.Equal(3, existing.CategoryId);
        Assert.Equal(5, result.Id);
        repo.Verify(r => r.UpdateAsync(5, existing), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Missing_ThrowsKeyNotFound()
    {
        var repo = new Mock<IBaseRepository<Income, int>>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Income?)null);
        var service = NewIncomeService(repo);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(1, new IncomeCreateDto { Value = 1m }));
        Assert.Equal("Entity not found", ex.Message);
    }

    [Fact]
    public async Task UserService_UpdateAsync_PreservesRoleAndId_AndRehashesPassword()
    {
        var existing = new User
        {
            Id = 7, UserName = "old", Email = "old@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("OldPass#1"), Role = EUserRole.admin
        };
        var repo = new Mock<IBaseRepository<User, int>>();
        repo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(existing);
        var userRepo = new Mock<IUserRepository>();
        var service = new UserService(repo.Object, userRepo.Object, Mock.Of<IUnitOfWork>(), Mapper(),
            new InlineValidator<UserCreateDto>(), Mock.Of<ICategoryRepository>());

        await service.UpdateAsync(7, new UserCreateDto { UserName = "new", Email = "new@example.com", Password = "NewPass#1" });

        Assert.Equal(7, existing.Id);
        Assert.Equal(EUserRole.admin, existing.Role);
        Assert.Equal("new", existing.UserName);
        Assert.Equal("new@example.com", existing.Email);
        Assert.NotEqual("NewPass#1", existing.Password);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPass#1", existing.Password));
    }

    [Fact]
    public async Task UpdateAsync_InvalidDto_ThrowsValidation()
    {
        var existing = new Income { Id = 5, UserId = 2, CategoryId = 1, Value = 1m, Description = "old" };
        var repo = new Mock<IBaseRepository<Income, int>>();
        repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);
        var validator = new InlineValidator<IncomeCreateDto>();
        validator.RuleFor(x => x.Value).GreaterThan(0);
        var service = new IncomeService(repo.Object, Mock.Of<IUnitOfWork>(), Mapper(), validator);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateAsync(5, new IncomeCreateDto { Value = 0m }));
        Assert.Equal(1m, existing.Value);
        repo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<Income>()), Times.Never);
    }
}
