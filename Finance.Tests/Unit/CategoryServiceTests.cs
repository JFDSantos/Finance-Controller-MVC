using AutoMapper;
using Finance.Application.Exceptions;
using Finance.Application.Interfaces;
using Finance.Application.Mappings;
using Finance.Application.Services;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Finance.Tests.Unit;

public class CategoryServiceTests
{
    private static (CategoryService svc, Mock<IBaseRepository<Category, int>> repo, Mock<IUnitOfWork> uow) New(params Category[] existing)
    {
        var repo = new Mock<IBaseRepository<Category, int>>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(existing.ToList());
        foreach (var c in existing) repo.Setup(r => r.GetByIdAsync(c.Id)).ReturnsAsync(c);
        var uow = new Mock<IUnitOfWork>();
        var mapper = new MapperConfiguration(c => c.AddProfile<FinanceMappingProfile>(), NullLoggerFactory.Instance).CreateMapper();
        var svc = new CategoryService(repo.Object, uow.Object, mapper, new InlineValidator<CategoryCreateDto>(), new Mock<IMovementReadRepository>().Object);
        return (svc, repo, uow);
    }

    [Fact]
    public async Task AddCategory_DuplicateName_ThrowsConflictCategoryNameTaken()
    {
        var (svc, repo, uow) = New(new Category { Id = 1, Name = "Lazer" });
        var ex = await Assert.ThrowsAsync<ConflictException>(() => svc.AddAsync(new CategoryCreateDto { Name = "Lazer" }));
        Assert.Equal("CATEGORY_NAME_TAKEN", ex.Code);
        Assert.Equal("A category with this name already exists.", ex.Message);
        repo.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
        uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task AddCategory_DuplicateNameDifferentCase_ThrowsConflict()
    {
        var (svc, _, _) = New(new Category { Id = 1, Name = "Lazer" });
        var ex = await Assert.ThrowsAsync<ConflictException>(() => svc.AddAsync(new CategoryCreateDto { Name = "  lazer " }));
        Assert.Equal("CATEGORY_NAME_TAKEN", ex.Code);
    }

    [Fact]
    public async Task AddCategory_TrimsNameBeforeSaving()
    {
        var (svc, repo, _) = New();
        await svc.AddAsync(new CategoryCreateDto { Name = "  Viagens " });
        repo.Verify(r => r.AddAsync(It.Is<Category>(c => c.Name == "Viagens")), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_RenameToExisting_ThrowsConflict()
    {
        var (svc, repo, uow) = New(new Category { Id = 1, Name = "Lazer" }, new Category { Id = 2, Name = "Casa" });
        var ex = await Assert.ThrowsAsync<ConflictException>(() => svc.UpdateAsync(2, new CategoryCreateDto { Name = "LAZER" }));
        Assert.Equal("CATEGORY_NAME_TAKEN", ex.Code);
        uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateCategory_SameNameOwnId_Succeeds()
    {
        var (svc, _, uow) = New(new Category { Id = 1, Name = "Lazer" }, new Category { Id = 2, Name = "Casa" });
        var result = await svc.UpdateAsync(1, new CategoryCreateDto { Name = "lazer" });
        Assert.Equal("lazer", result.Name);
        uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCategory_UniqueViolationFromDb_ThrowsConflict()
    {
        var (svc, _, uow) = New();
        uow.Setup(u => u.CommitAsync()).ThrowsAsync(new UniqueConstraintException(new Exception("23505")));
        var ex = await Assert.ThrowsAsync<ConflictException>(() => svc.AddAsync(new CategoryCreateDto { Name = "Nova" }));
        Assert.Equal("CATEGORY_NAME_TAKEN", ex.Code);
    }

    [Fact]
    public async Task UpdateCategory_UniqueViolationFromDb_ThrowsConflict()
    {
        var (svc, _, uow) = New(new Category { Id = 1, Name = "Lazer" });
        uow.Setup(u => u.CommitAsync()).ThrowsAsync(new UniqueConstraintException(new Exception("23505")));
        var ex = await Assert.ThrowsAsync<ConflictException>(() => svc.UpdateAsync(1, new CategoryCreateDto { Name = "Outra" }));
        Assert.Equal("CATEGORY_NAME_TAKEN", ex.Code);
    }
}
