using Finance.Application.ViewModel;
using Finance.Infrastructure.Data;
using Finance.Domain.Models;
using Finance.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly FinanceContext _context;
        public CategoryRepository(FinanceContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }


        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task UpdateAsync(int id, Category category)
        {
            var categoryFind = await _context.Categories.FindAsync(id);

            if (categoryFind != null)
            {
                _context.Entry(categoryFind).CurrentValues.SetValues(category);
            }

        }

    } 
}
