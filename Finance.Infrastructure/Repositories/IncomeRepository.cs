using Finance.Infrastructure.Data;
using Finance.Domain.Models;
using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Repositories
{
    public class IncomeRepository : IIncomeRepository
    {
        private readonly FinanceContext _context;
        public IncomeRepository(FinanceContext context) { 
            _context = context;
        }

        public async Task AddAsync(Income dto)
        {
            _context.Add(dto);
            await _context.SaveChangesAsync();

        }

        public Task DeleteAsync(int id)
        {
            var income =  _context.Incomes.Include(i => i.Category).FirstOrDefault(i => i.id == id);

            if (income != null)
            {
                _context.Incomes.Remove(income);
                return _context.SaveChangesAsync();
            }

            throw new KeyNotFoundException("Income not found");
        }

        public async Task<IEnumerable<Income>> GetAllAsync()
        {
            var incomes = await _context.Incomes.Include(i => i.Category).Select(c => new Income
            {
                id = c.id,
                Category = c.Category,
                categoryId = c.categoryId,
                description = c.description,
                isAppellant = c.isAppellant,
                movimentDate = c.movimentDate,
                value = c.value

            }).ToListAsync();

            if (incomes != null)
            {
                return incomes;
            }

            throw new KeyNotFoundException("Incomes not found");
        }

        public async Task<Income> GetByIdAsync(int id)
        {
            var incomes = await _context.Incomes.Include(i => i.Category).Select(c => new Income
            {
                id = c.id,
                Category = c.Category,
                categoryId = c.categoryId,
                description = c.description,
                isAppellant = c.isAppellant,
                movimentDate = c.movimentDate,
                value = c.value

            }).FirstOrDefaultAsync(i => i.id == id);

            if (incomes != null) { 
                return incomes;
            }

            throw new KeyNotFoundException("Income not Found");
        }

        public async Task<Income> UpdateAsync(int id, Income dto)
        {
            var income = await _context.Incomes.Include(i => i.Category).FirstOrDefaultAsync(i => i.id == id);



            if (income != null)
            {
                income.id = id;
                income.value = dto.value;
                income.movimentDate = dto.movimentDate;
                income.description = dto.description;
                income.categoryId = dto.categoryId;
                income.isAppellant = dto.isAppellant;

                _context.Incomes.Update(income);
                await _context.SaveChangesAsync();

                return new Income
                {
                    id = id,
                    value = income.value,
                    movimentDate = income.movimentDate,
                    description = income.description,
                    categoryId = income.categoryId,
                    isAppellant = income.isAppellant
                };
            }

            throw new KeyNotFoundException("Income not Found");
        }
    }
}
