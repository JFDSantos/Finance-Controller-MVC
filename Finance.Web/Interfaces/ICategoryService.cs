using Finance.Web.Models;
using Finance.Web.ViewModel;

namespace Finance.Web.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategorySelectDto>> GetAllAsync();
        Task<CategorySelectDto> GetByIdAsync(int id);
        Task AddAsync(Category dto);
        Task DeleteAsync(int IdTransaction);
        Task<CategorySelectDto> UpdateAsync(int IdTransaction, CategoryCreateDto dto);
    }
}
