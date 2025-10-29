using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
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
