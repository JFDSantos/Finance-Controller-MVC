using Finance.Infrastructure.Data;
using Finance.Domain.Models;
using Finance.Application.Interfaces;
using Finance.Application;
using Microsoft.EntityFrameworkCore;
using Finance.Application.ViewModel;

namespace Finance.Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly FinanceContext _context;
        public ExpenseRepository(FinanceContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Expense dto)
        {
            _context.Add(dto);
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

        public async Task<IEnumerable<ExpenseDto>> GetAllAsync()
        {
            var expenses = await _context.Expenses.Include(i => i.Category).Select(e => new ExpenseDto
            {
                CategoryName = e.Category.Name,
                CategoryId = e.Category.Id,
                Description = e.description,
                Id = e.id,
                IsAppellant = e.isAppellant,
                MovimentDate = e.movimentDate,
                Value = e.value
            }).ToListAsync();

            if (expenses != null)
            {
                return expenses;
            }

            throw new KeyNotFoundException("Expenses not found");
        }

        public async Task<ExpenseDto> GetByIdAsync(int id)
        {
            var expense = await _context.Expenses.Include(i => i.Category).Select(e => new ExpenseDto
            {
                CategoryName = e.Category.Name,
                CategoryId = e.Category.Id,
                Description = e.description,
                Id = e.id,
                IsAppellant = e.isAppellant,
                MovimentDate = e.movimentDate,
                Value = e.value
            }).FirstOrDefaultAsync(i => i.Id == id);

            if(expense != null)
            {
                return expense;
            }

            throw new KeyNotFoundException("Expense not found");
        }

        public async Task<ExpenseDto> UpdateAsync(int id, ExpenseCreateDto dto)
        {
            var expense = await _context.Expenses.Include(i => i.Category).FirstOrDefaultAsync(i => i.id == id);

            if (expense != null) 
            {
                expense.id = id;
                expense.value = dto.Value;
                expense.description = dto.Description;
                expense.movimentDate = dto.MovimentDate;
                expense.isAppellant = dto.IsAppellant;
                expense.categoryId = dto.CategoryId;

                _context.Update(expense);
                await _context.SaveChangesAsync();

                return new ExpenseDto
                {
                    Id = id,
                    CategoryId = expense.categoryId,
                    Value = expense.value,
                    Description = expense.description,
                    MovimentDate = expense.movimentDate,
                    IsAppellant = expense.isAppellant,
                    CategoryName = expense.Category.Name
                };
            }

            throw new KeyNotFoundException("Expense not found");
        }
    }
}
