using Finance.Web.Models;
using Finance.Web.ViewModel;

namespace Finance.Web.Patterns.Interfaces
{
    public interface IIncomeService
    {
        Task<IEnumerable<IncomeDto>> GetAllAsync();
        Task<IncomeDto> GetByIdAsync(int id);
        Task AddAsync(IncomeCreateDto dto);
        Task DeleteAsync(int id);
        Task<IncomeDto> UpdateAsync(int id, IncomeCreateDto dto);
    }
}
