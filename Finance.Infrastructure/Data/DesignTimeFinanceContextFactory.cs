using Finance.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Finance.Infrastructure.Data
{
    /// <summary>Used only by dotnet-ef tooling to build the context at design time.</summary>
    public class DesignTimeFinanceContextFactory : IDesignTimeDbContextFactory<FinanceContext>
    {
        private sealed class DesignTimeUser : ICurrentUser
        {
            public int UserId => 0;
        }

        public FinanceContext CreateDbContext(string[] args)
        {
            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? Environment.GetEnvironmentVariable("TEST_DB_CONNECTION")
                ?? "Host=localhost;Database=finance;Username=postgres";
            var options = new DbContextOptionsBuilder<FinanceContext>().UseNpgsql(conn).Options;
            return new FinanceContext(options, new DesignTimeUser());
        }
    }
}
