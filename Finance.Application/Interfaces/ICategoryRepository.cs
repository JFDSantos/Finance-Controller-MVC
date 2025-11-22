using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task AddAsync(Category dto);
        Task DeleteAsync(int IdTransaction);
        Task<Category> UpdateAsync(int IdTransaction, Category dto);
    }
}
