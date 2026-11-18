using AutoMapper;
using Finance.Application.Interfaces;
using Finance.Application.Mappings;
using Finance.Application.Services;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using Finance.Infrastructure.Data;
using Finance.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Finance.Tests.Integration;

[Collection("Database")]
public class RepositoryIsolationTests
{
    private sealed class StubCurrentUser : ICurrentUser
    {
        public int UserId { get; set; }
    }

    private static string Conn =>
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Host=localhost;Database=finance_test;Username=postgres;Password=postgres";

    private static FinanceContext NewContext(int userId) =>
        new(new DbContextOptionsBuilder<FinanceContext>().UseNpgsql(Conn).Options,
            new StubCurrentUser { UserId = userId });

    private sealed class Scope
    {
        public int A, B;
        public int CatA, CatB, IncomeA, IncomeB;
    }

    private static async Task<Scope> SeedAsync()
    {
        await using (var reset = NewContext(0))
        {
            await reset.Database.EnsureDeletedAsync();
            await reset.Database.MigrateAsync();
        }
        await using var db = NewContext(0);
        var ua = new User { UserName = "a", Password = "x", Email = "a@example.com" };
        var ub = new User { UserName = "b", Password = "x", Email = "b@example.com" };
        db.User.AddRange(ua, ub);
        await db.SaveChangesAsync();
        var ca = new Category { Name = "CatA", UserId = ua.Id };
        var cb = new Category { Name = "CatB", UserId = ub.Id };
        db.Categories.AddRange(ca, cb);
        await db.SaveChangesAsync();
        var ia = new Income { CategoryId = ca.Id, UserId = ua.Id, Value = 10m, MovimentDate = new DateOnly(2026, 1, 2) };
        var ib = new Income { CategoryId = cb.Id, UserId = ub.Id, Value = 20m, MovimentDate = new DateOnly(2026, 1, 3) };
        db.Incomes.AddRange(ia, ib);
        await db.SaveChangesAsync();
        return new Scope { A = ua.Id, B = ub.Id, CatA = ca.Id, CatB = cb.Id, IncomeA = ia.Id, IncomeB = ib.Id };
    }

    private static (FinanceContext ctx, IncomeRepository income, CategoryRepository cat) Repos(int userId)
    {
        var ctx = NewContext(userId);
        var cu = new StubCurrentUser { UserId = userId };
        return (ctx, new IncomeRepository(ctx, cu), new CategoryRepository(ctx, cu));
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOwnCategoriesAndMoviments()
    {
        var s = await SeedAsync();
        var (ctxA, incA, catA) = Repos(s.A);
        var (ctxB, incB, catB) = Repos(s.B);
        await using var _a = ctxA; await using var _b = ctxB;

        Assert.Equal(new[] { s.CatA }, (await catA.GetAllAsync()).Select(c => c.Id).ToArray());
        Assert.Equal(new[] { s.CatB }, (await catB.GetAllAsync()).Select(c => c.Id).ToArray());
        Assert.Equal(new[] { s.IncomeA }, (await incA.GetAllAsync()).Select(c => c.Id).ToArray());
        Assert.Equal(new[] { s.IncomeB }, (await incB.GetAllAsync()).Select(c => c.Id).ToArray());
    }

    [Fact]
    public async Task GetById_OtherUsersRecord_ReturnsNull()
    {
        var s = await SeedAsync();
        var (ctxA, incA, catA) = Repos(s.A);
        var (ctxB, incB, catB) = Repos(s.B);
        await using var _a = ctxA; await using var _b = ctxB;

        // seeding assertions: the owner can read them
        Assert.NotNull(await incA.GetByIdAsync(s.IncomeA));
        Assert.NotNull(await catA.GetByIdAsync(s.CatA));

        Assert.Null(await incB.GetByIdAsync(s.IncomeA));
        Assert.Null(await catB.GetByIdAsync(s.CatA));
    }

    [Fact]
    public async Task GetById_OtherUsersRecord_ReturnsNull_EvenWhenAlreadyTrackedElsewhere()
    {
        var s = await SeedAsync();
        var (ctxB, incB, _) = Repos(s.B);
        await using var _b = ctxB;
        Assert.NotNull(await incB.GetByIdAsync(s.IncomeB));
        Assert.Null(await incB.GetByIdAsync(s.IncomeA));
    }

    [Fact]
    public async Task Add_WithUserIdZero_StoresCurrentUserId()
    {
        var s = await SeedAsync();
        var (ctxA, _, catA) = Repos(s.A);
        await using var _a = ctxA;
        var cat = new Category { Name = "NewForA" };
        Assert.Equal(0, cat.UserId);
        await catA.AddAsync(cat);
        await ctxA.SaveChangesAsync();
        Assert.Equal(s.A, cat.UserId);

        await using var check = NewContext(s.A);
        Assert.Equal(s.A, (await check.Categories.SingleAsync(c => c.Name == "NewForA")).UserId);
    }

    [Fact]
    public async Task Add_WithExplicitUserId_KeepsItWithoutReadingCurrentUser()
    {
        var s = await SeedAsync();
        await using var ctx = NewContext(0); // unauthenticated stub
        var repo = new CategoryRepository(ctx, new ThrowingCurrentUser());
        await repo.AddAsync(new Category { Name = "Seeded", UserId = s.B });
        await ctx.SaveChangesAsync();

        await using var check = NewContext(s.B);
        Assert.Contains(await check.Categories.ToListAsync(), c => c.Name == "Seeded" && c.UserId == s.B);
    }

    private sealed class ThrowingCurrentUser : ICurrentUser
    {
        public int UserId => throw new InvalidOperationException("must not be read");
    }

    [Fact]
    public async Task Delete_ByOtherUser_DeletesNothing()
    {
        var s = await SeedAsync();
        var (ctxB, incB, _) = Repos(s.B);
        await using var _b = ctxB;
        var (ctxA, incA, _) = Repos(s.A);
        await using var _a = ctxA;

        Assert.NotNull(await incA.GetByIdAsync(s.IncomeA)); // seeded
        await incB.DeleteAsync(s.IncomeA);
        await ctxB.SaveChangesAsync();

        await using var check = NewContext(s.A);
        Assert.Single(await check.Incomes.Where(i => i.Id == s.IncomeA).ToListAsync());
    }

    [Fact]
    public async Task Service_Update_PreservesUserId_AndOtherUserCannotUpdate()
    {
        var s = await SeedAsync();
        var mapper = new MapperConfiguration(c => c.AddProfile<FinanceMappingProfile>(), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance).CreateMapper();
        var dto = new IncomeCreateDto
        {
            Description = "updated", Value = 77m, CategoryId = s.CatA, MovimentDate = new DateOnly(2026, 5, 6)
        };

        // other user: not found, nothing changes
        await using (var ctxB = NewContext(s.B))
        {
            var svcB = new IncomeService(new IncomeRepository(ctxB, new StubCurrentUser { UserId = s.B }),
                new UnitOfWork(ctxB), mapper, new InlineValidator<IncomeCreateDto>());
            await Assert.ThrowsAsync<KeyNotFoundException>(() => svcB.UpdateAsync(s.IncomeA, dto));
        }
        await using (var chk = NewContext(s.A))
            Assert.Equal(10m, (await chk.Incomes.SingleAsync(i => i.Id == s.IncomeA)).Value);

        // owner: values applied, Id and UserId preserved
        await using (var ctxA = NewContext(s.A))
        {
            var svcA = new IncomeService(new IncomeRepository(ctxA, new StubCurrentUser { UserId = s.A }),
                new UnitOfWork(ctxA), mapper, new InlineValidator<IncomeCreateDto>());
            var res = await svcA.UpdateAsync(s.IncomeA, dto);
            Assert.Equal(s.IncomeA, res.Id);
        }
        await using var after = NewContext(s.A);
        var row = await after.Incomes.SingleAsync(i => i.Id == s.IncomeA);
        Assert.Equal(s.A, row.UserId);
        Assert.Equal(77m, row.Value);
        Assert.Equal("updated", row.Description);
    }
}
