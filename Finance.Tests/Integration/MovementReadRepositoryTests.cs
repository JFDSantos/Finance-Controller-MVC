using Finance.Application.Interfaces;
using Finance.Domain.Models;
using Finance.Infrastructure.Data;
using Finance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finance.Tests.Integration;

[Collection("Database")]
public class MovementReadRepositoryTests
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

    [Fact]
    public async Task AnyByCategoryAsync_IsScopedToCurrentUser()
    {
        await using (var reset = NewContext(0))
        {
            await reset.Database.EnsureDeletedAsync();
            await reset.Database.MigrateAsync();
        }
        int a, b, usedA, emptyA, catB;
        await using (var db = NewContext(0))
        {
            var ua = new User { UserName = "a", Password = "x", Email = "a@example.com" };
            var ub = new User { UserName = "b", Password = "x", Email = "b@example.com" };
            db.User.AddRange(ua, ub);
            await db.SaveChangesAsync();
            var ca = new Category { Name = "Used", UserId = ua.Id };
            var ce = new Category { Name = "Empty", UserId = ua.Id };
            var cb = new Category { Name = "B", UserId = ub.Id };
            db.Categories.AddRange(ca, ce, cb);
            await db.SaveChangesAsync();
            db.Incomes.Add(new Income { CategoryId = ca.Id, UserId = ua.Id, Value = 10m, MovimentDate = new DateOnly(2026, 1, 2) });
            db.Incomes.Add(new Income { CategoryId = cb.Id, UserId = ub.Id, Value = 20m, MovimentDate = new DateOnly(2026, 1, 3) });
            await db.SaveChangesAsync();
            a = ua.Id; b = ub.Id; usedA = ca.Id; emptyA = ce.Id; catB = cb.Id;
        }

        // seeding assertions
        await using (var chk = NewContext(0))
        {
            Assert.Equal(2, await chk.Moviments.IgnoreQueryFilters().CountAsync());
            Assert.Equal(3, await chk.Categories.IgnoreQueryFilters().CountAsync());
        }

        await using var ctxA = NewContext(a);
        var repoA = new MovementReadRepository(ctxA);
        Assert.True(await repoA.AnyByCategoryAsync(usedA));
        Assert.False(await repoA.AnyByCategoryAsync(emptyA));
        Assert.False(await repoA.AnyByCategoryAsync(catB));

        await using var ctxB = NewContext(b);
        Assert.True(await new MovementReadRepository(ctxB).AnyByCategoryAsync(catB));
        Assert.False(await new MovementReadRepository(ctxB).AnyByCategoryAsync(usedA));
    }
}
