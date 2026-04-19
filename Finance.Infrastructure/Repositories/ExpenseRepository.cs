using Finance.Infrastructure.Data;
using Finance.Domain.Models;
using Finance.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly FinanceContext _context;
        public ExpenseRepository(FinanceContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Expense expense)
        {
           await _context.Expenses.AddAsync(expense);
        }

        public async Task DeleteAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense != null)
            {
                _context.Expenses.Remove(expense);
            }
        }

        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            return await _context.Expenses.Include(i => i.Category).AsNoTracking().ToListAsync();
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses.Include(i => i.Category).AsNoTracking().FirstOrDefaultAsync(i => i.id == id);
        }

        public async Task UpdateAsync(int id, Expense expense)
        {
            var expenseFind = await _context.Expenses.FindAsync(id);

            if (expenseFind != null)
            {
                _context.Entry(expenseFind).CurrentValues.SetValues(expense);
            }

        }
    }
}
