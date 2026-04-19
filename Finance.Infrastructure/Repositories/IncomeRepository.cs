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

        public async Task AddAsync(Income income)
        {
            await _context.Incomes.AddAsync(income);
        }

        public async Task DeleteAsync(int id)
        {
            var income =  await _context.Incomes.FindAsync(id);

            if (income != null)
            {
                _context.Incomes.Remove(income);
            }
        }
        public async Task<IEnumerable<Income?>> GetAllAsync()
        {
            return await _context.Incomes.Include(i => i.Category).AsNoTracking().ToListAsync();
        }

        public async Task<Income?> GetByIdAsync(int id)
        {
            return await _context.Incomes.Include(i => i.Category).AsNoTracking().FirstOrDefaultAsync(i => i.id == id);
        }

        public async Task UpdateAsync(int id, Income dto)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income != null)
            {
                _context.Entry(income).CurrentValues.SetValues(dto);
            }
        }
    }
}
