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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define o campo "type" como discriminador explícito
            modelBuilder.Entity<Moviment>()
                .HasDiscriminator<MovimentType>("type")
                .HasValue<Moviment>(MovimentType.Base)
                .HasValue<Income>(MovimentType.Income)
                .HasValue<Expense>(MovimentType.Expense);

            // Configura relacionamento Moviment → Category
            modelBuilder.Entity<Moviment>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Moviments)
                .HasForeignKey(m => m.categoryId);
        }
    }
}
