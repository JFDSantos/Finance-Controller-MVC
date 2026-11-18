using Finance.Application.Interfaces;
using Finance.Domain.Models;
using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finance.Tests.Integration;

[Collection("Database")]
public class SchemaTests
{
    private sealed class StubCurrentUser : ICurrentUser
    {
        public int UserId { get; set; }
    }

    private static string Conn =>
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
        ?? "Host=localhost;Database=finance_test;Username=postgres;Password=postgres";

    private static FinanceContext NewContext(int userId = 0) =>
        new(new DbContextOptionsBuilder<FinanceContext>().UseNpgsql(Conn).Options,
            new StubCurrentUser { UserId = userId });

    private static async Task FreshDbAsync()
    {
        await using var db = NewContext();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    private static User NewUser(string name) =>
        new() { UserName = name, Password = "x", Email = $"{name}@example.com" };

    [Fact]
    public async Task Migrations_ApplyToEmptyPostgres()
    {
        await FreshDbAsync();
        await using var db = NewContext();
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        var tables = new List<string>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "select table_name from information_schema.tables where table_schema='public'";
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) tables.Add(r.GetString(0));
        }
        Assert.Contains("Moviments", tables);
        Assert.Contains("Categories", tables);
        Assert.Contains("User", tables);
    }

    [Fact]
    public async Task CategoryName_UniquePerUser()
    {
        await FreshDbAsync();
        await using var db = NewContext();
        var u1 = NewUser("u1"); var u2 = NewUser("u2");
        db.User.AddRange(u1, u2);
        await db.SaveChangesAsync();

        db.Categories.Add(new Category { Name = "Food", UserId = u1.Id });
        await db.SaveChangesAsync();

        // different user, same name: ok
        db.Categories.Add(new Category { Name = "Food", UserId = u2.Id });
        await db.SaveChangesAsync();

        // same user, same name: rejected
        await using var db2 = NewContext();
        db2.Categories.Add(new Category { Name = "Food", UserId = u1.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => db2.SaveChangesAsync());
    }

    [Fact]
    public async Task DeletingUser_CascadesToCategoriesAndMoviments()
    {
        await FreshDbAsync();
        await using var db = NewContext();
        var u = NewUser("u1");
        db.User.Add(u);
        await db.SaveChangesAsync();
        var cat = new Category { Name = "Food", UserId = u.Id };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();
        db.Incomes.Add(new Income { CategoryId = cat.Id, UserId = u.Id, Value = 10.5m, MovimentDate = new DateOnly(2026, 1, 2) });
        await db.SaveChangesAsync();

        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"User\" WHERE \"Id\" = {0}", u.Id);

        await using var check = NewContext();
        Assert.Equal(0, await check.Categories.IgnoreQueryFilters().CountAsync());
        Assert.Equal(0, await check.Moviments.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task QueryFilter_ScopesToCurrentUser()
    {
        await FreshDbAsync();
        await using var seed = NewContext();
        var u1 = NewUser("u1"); var u2 = NewUser("u2");
        seed.User.AddRange(u1, u2);
        await seed.SaveChangesAsync();
        seed.Categories.AddRange(new Category { Name = "A", UserId = u1.Id }, new Category { Name = "B", UserId = u2.Id });
        await seed.SaveChangesAsync();

        await using var asU1 = NewContext(u1.Id);
        var names = await asU1.Categories.Select(c => c.Name).ToListAsync();
        Assert.Equal(new[] { "A" }, names);
    }

    [Fact]
    public async Task ValueColumn_IsDecimal18_2_AndDateColumnIsDate()
    {
        await FreshDbAsync();
        await using var db = NewContext();
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "select column_name||':'||data_type||':'||coalesce(numeric_precision::text,'')||':'||coalesce(numeric_scale::text,'') from information_schema.columns where table_name='Moviments' and column_name in ('Value','MovimentDate')";
        var cols = new List<string>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) cols.Add(r.GetString(0));
        Assert.Contains("Value:numeric:18:2", cols);
        Assert.Contains("MovimentDate:date::", cols);
    }
}

[CollectionDefinition("Database", DisableParallelization = true)]
public class DatabaseCollection { }
