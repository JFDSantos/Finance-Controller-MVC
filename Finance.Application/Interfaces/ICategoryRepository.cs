using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategorySelectDto>> GetAllAsync();
        Task<CategorySelectDto> GetByIdAsync(int id);
        Task AddAsync(Category dto);
        Task DeleteAsync(int IdTransaction);
        Task<CategorySelectDto> UpdateAsync(int IdTransaction, CategoryCreateDto dto);
    }
}
