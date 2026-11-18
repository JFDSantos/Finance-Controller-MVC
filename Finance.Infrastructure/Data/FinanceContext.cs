using Finance.Application.Interfaces;
using Finance.Domain.Models;
using Finance.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Data
{
    public class FinanceContext : DbContext
    {
        private readonly ICurrentUser _currentUser;

        public FinanceContext(DbContextOptions<FinanceContext> options, ICurrentUser currentUser) : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Moviment> Moviments { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(u =>
            {
                u.ToTable("User");
                // Application code (Task 8) stores emails lowercase, so a plain unique index is case-insensitive in practice.
                u.HasIndex(x => x.Email).IsUnique();
            });

            modelBuilder.Entity<Moviment>(m =>
            {
                m.HasDiscriminator(x => x.Type)
                    .HasValue<Income>(EMovimentType.Income)
                    .HasValue<Expense>(EMovimentType.Expense);
                m.Property(x => x.Value).HasPrecision(18, 2);
                m.Property(x => x.MovimentDate).HasColumnType("date");

                // NoAction (not Restrict): Postgres RESTRICT is checked immediately and would break
                // the cascade that removes all of a user's data.
                m.HasOne(x => x.Category)
                    .WithMany(c => c.Moviments)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);

                m.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

                // Reads _currentUser on the context instance, so it is evaluated per query.
                m.HasQueryFilter(x => x.UserId == _currentUser.UserId);
            });

            modelBuilder.Entity<Category>(c =>
            {
                c.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
                c.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
                c.HasQueryFilter(x => x.UserId == _currentUser.UserId);
            });
        }
    }
}
