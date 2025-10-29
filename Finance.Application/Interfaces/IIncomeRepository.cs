using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IIncomeRepository
    {
        Task<IEnumerable<IncomeDto>> GetAllAsync();
        Task<IncomeDto> GetByIdAsync(int id);
        Task AddAsync(Income dto);
        Task DeleteAsync(int id);
        Task<IncomeDto> UpdateAsync(int id, IncomeCreateDto dto);
    }
}
