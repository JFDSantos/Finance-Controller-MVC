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
            _context.Add(expense);
            await _context.SaveChangesAsync();
        }

        public Task DeleteAsync(int id)
        {
            var expense = _context.Expenses.Find(id);

            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                return _context.SaveChangesAsync();
            }

            throw new KeyNotFoundException("Expense not found");
        }

        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            var expenses = await _context.Expenses.Include(i => i.Category).Select(e => new Expense
            {
                Category = e.Category,
                categoryId = e.categoryId,
                description = e.description,
                id = e.id,
                isAppellant = e.isAppellant,
                movimentDate = e.movimentDate,
                value = e.value

            }).ToListAsync();

            if (expenses != null)
            {
                return expenses;
            }

            throw new KeyNotFoundException("Expenses not found");
        }

        public async Task<Expense> GetByIdAsync(int id)
        {
            var expense = await _context.Expenses.Include(i => i.Category).Select(e => new Expense
            {
                Category = e.Category,
                categoryId = e.categoryId,
                description = e.description,
                id = e.id,
                isAppellant = e.isAppellant,
                movimentDate = e.movimentDate,
                value = e.value

            }).FirstOrDefaultAsync(i => i.id == id);

            if(expense != null)
            {
                return expense;
            }

            throw new KeyNotFoundException("Expense not found");
        }

        public async Task<Expense> UpdateAsync(int id, Expense expenseCreate)
        {
            var expense = await _context.Expenses.Include(i => i.Category).FirstOrDefaultAsync(i => i.id == id);

            if (expense != null) 
            {
                expense.id = id;
                expense.value = expenseCreate.value;
                expense.description = expenseCreate.description;
                expense.movimentDate = expenseCreate.movimentDate;
                expense.isAppellant = expenseCreate.isAppellant;
                expense.categoryId = expenseCreate.categoryId;

                _context.Update(expense);
                await _context.SaveChangesAsync();

                return new Expense
                {
                    id = id,
                    categoryId = expense.categoryId,
                    value = expense.value,
                    description = expense.description,
                    movimentDate = expense.movimentDate,
                    isAppellant = expense.isAppellant,
                    Category = expense.Category
                };
            }

            throw new KeyNotFoundException("Expense not found");
        }
    }
}
