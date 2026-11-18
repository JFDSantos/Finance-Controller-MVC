using Finance.Application.Services;
using Finance.Domain.Models;
using Finance.Domain.Models.Enums;
using Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Seed
{
    /// <summary>
    /// Creates the public demo account with sample data. Idempotent. Runs at startup where there is no HTTP user,
    /// so it never reads ICurrentUser and sets UserId explicitly on every row.
    /// </summary>
    public static class DemoSeeder
    {
        public const string DemoEmail = "demo@financecontroller.app";

        public static async Task SeedAsync(FinanceContext context, string demoPassword)
        {
            if (string.IsNullOrWhiteSpace(demoPassword))
                throw new ArgumentException("Demo password must not be empty.", nameof(demoPassword));

            var user = await context.User.FirstOrDefaultAsync(u => u.Email == DemoEmail);
            if (user == null)
            {
                user = new User
                {
                    UserName = "Demo",
                    Email = DemoEmail,
                    Password = BCrypt.Net.BCrypt.HashPassword(demoPassword),
                    Role = EUserRole.user
                };
                context.User.Add(user);
                await context.SaveChangesAsync();
            }

            var existing = await context.Categories.IgnoreQueryFilters()
                .Where(c => c.UserId == user.Id).ToListAsync();
            foreach (var name in DefaultCategories.Names.Where(n => existing.All(c => c.Name != n)))
            {
                var cat = new Category { Name = name, UserId = user.Id };
                context.Categories.Add(cat);
                existing.Add(cat);
            }
            await context.SaveChangesAsync();

            if (await context.Moviments.IgnoreQueryFilters().AnyAsync(m => m.UserId == user.Id))
                return;

            int Cat(string name) => existing.First(c => c.Name == name).Id;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var firstOfMonth = new DateOnly(today.Year, today.Month, 1);

            for (var back = 0; back < 3; back++)
            {
                var start = firstOfMonth.AddMonths(-back);
                var last = DateOnly.FromDayNumber(start.AddMonths(1).DayNumber - 1);
                DateOnly Day(int d) => start.AddDays(Math.Min(d, last.Day) - 1);
                bool Past(DateOnly d) => d <= today;

                void Inc(int day, string desc, decimal v, string cat, bool rec = false)
                {
                    var d = Day(day);
                    if (Past(d)) context.Incomes.Add(new Income { UserId = user.Id, CategoryId = Cat(cat), Description = desc, Value = v, MovimentDate = d, IsRecurring = rec });
                }
                void Exp(int day, string desc, decimal v, string cat, bool rec = false)
                {
                    var d = Day(day);
                    if (Past(d)) context.Expenses.Add(new Expense { UserId = user.Id, CategoryId = Cat(cat), Description = desc, Value = v, MovimentDate = d, IsRecurring = rec });
                }

                Inc(5, "Monthly salary", 5200m, "Salário", true);
                if (back != 1) Inc(18, "Freelance project", 850m + back * 150m, "Outros");
                Exp(6, "Rent", 1800m, "Moradia", true);
                Exp(8, "Internet and phone", 129.90m, "Moradia", true);
                Exp(3, "Groceries", 342.50m - back * 20m, "Alimentação");
                Exp(12, "Restaurant", 96.80m + back * 12m, "Alimentação");
                Exp(20, "Groceries", 287.35m, "Alimentação");
                Exp(10, "Fuel", 210m, "Transporte");
                Exp(15, "Ride apps", 64.20m + back * 5m, "Transporte");
                Exp(14, "Pharmacy", 78.45m, "Saúde");
                Exp(22, "Cinema and streaming", 59.90m, "Lazer");
                Exp(25, "Weekend trip", 240m - back * 30m, "Lazer");
                Exp(27, "Miscellaneous", 45m, "Outros");
            }
            await context.SaveChangesAsync();
        }
    }
}
