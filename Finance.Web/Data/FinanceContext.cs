using Finance.Web.Models;
using Finance.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finance.Web.Data
{
    public class FinanceContext : DbContext
    {
        public FinanceContext(DbContextOptions<FinanceContext> options) : base(options){ }

        public DbSet<Moviment> Moviments { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define o campo "type" como discriminador explícito
            modelBuilder.Entity<Moviment>()
                .HasDiscriminator<EMovimentType>("type")
                .HasValue<Moviment>(EMovimentType.Base)
                .HasValue<Income>(EMovimentType.Income)
                .HasValue<Expense>(EMovimentType.Expense);

            // Configura relacionamento Moviment → Category
            modelBuilder.Entity<Moviment>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Moviments)
                .HasForeignKey(m => m.categoryId);
        }
    }
}
