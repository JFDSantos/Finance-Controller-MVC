using Finance.Web.Data;
using Finance.Web.Models;
using Finance.Web.Patterns.Interfaces;
using Finance.Web.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finance.Web.Patterns.Repositories
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

        public  Task DeleteAsync(int id)
        {
            var income =  _context.Incomes.Include(i => i.Category).FirstOrDefault(i => i.id == id);

            if (income != null)
            {
                _context.Incomes.Remove(income);
                return _context.SaveChangesAsync();
            }

            throw new KeyNotFoundException("Income not found");
        }

        public async Task<IEnumerable<IncomeDto>> GetAllAsync()
        {
            var incomes = await _context.Incomes.Select(c => new IncomeDto
            {
                Id = c.id,
                CategoryId = c.categoryId,
                CategoryName = c.Category.Name,
                Description = c.description,
                IsAppellant = c.isAppellant,
                MovimentDate = c.movimentDate,
                Value = c.value

            }).ToListAsync();

            if (incomes != null)
            {
                return incomes;
            }

            throw new KeyNotFoundException("Incomes not found");
        }

        public async Task<IncomeDto> GetByIdAsync(int id)
        {
            var incomes = await _context.Incomes.Include(i => i.Category).Select(c => new IncomeDto
            {
                Id = c.id,
                CategoryId = c.categoryId,
                CategoryName = c.Category.Name,
                Description = c.description,
                IsAppellant = c.isAppellant,
                MovimentDate = c.movimentDate,
                Value = c.value

            }).FirstOrDefaultAsync(i => i.Id == id);

            if (incomes != null) { 
                return incomes;
            }

            throw new KeyNotFoundException("Income not Found");
        }

        public async Task<IncomeDto> UpdateAsync(int id, IncomeCreateDto dto)
        {
            var income = await _context.Incomes.Include(i => i.Category).FirstOrDefaultAsync(i => i.id == id);



            if (income != null)
            {
                income.id = id;
                income.value = dto.Value;
                income.movimentDate = dto.MovimentDate;
                income.description = dto.Description;
                income.categoryId = dto.CategoryId;
                income.isAppellant = dto.IsAppellant;

                _context.Incomes.Update(income);
                await _context.SaveChangesAsync();

                return new IncomeDto
                {
                    Id = id,
                    Value = income.value,
                    MovimentDate = income.movimentDate,
                    Description = income.description,
                    CategoryId = income.categoryId,
                    IsAppellant = income.isAppellant,
                    CategoryName = income.Category.Name
                };
            }

            throw new KeyNotFoundException("Income not Found");
        }
    }
}
