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

        public async Task AddAsync(Category dto)
        {
            _context.Add(dto);
            await _context.SaveChangesAsync();
        }
        public Task DeleteAsync(int IdTransaction)
        {
            var category = _context.Categories.Find(IdTransaction);
            if (category != null)
            {
                _context.Categories.Remove(category);
                return _context.SaveChangesAsync();
            }
                
            throw new KeyNotFoundException("Category not found");
        }
        public async Task<IEnumerable<CategorySelectDto>> GetAllAsync()
        {
            var categories = await _context.Categories.Select(c => new CategorySelectDto
            {
                Id = c.Id,
                Name = c.Name
            }).ToListAsync();

            if (categories == null)
            {
                throw new KeyNotFoundException("Categories not found");
            }

            return categories;
        }
        public async Task<CategorySelectDto> GetByIdAsync(int Id)
        {
            var categories = await _context.Categories.Select(c => new CategorySelectDto
            {
                Id = c.Id,
                Name = c.Name
            }).FirstOrDefaultAsync(i => i.Id == Id);

            if (categories == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            return categories;
        }
        public async Task<CategorySelectDto> UpdateAsync(int IdTransaction, CategoryCreateDto dto)
        {
            var categorie = await _context.Categories.FindAsync(IdTransaction);

            if (categorie == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            categorie.Id = IdTransaction;
            categorie.Name = dto.Name;

            _context.Categories.Update(categorie);
            await _context.SaveChangesAsync();

            return new CategorySelectDto
            {
                Id = categorie.Id,
                Name = categorie.Name
            };
        }

    } 
}
