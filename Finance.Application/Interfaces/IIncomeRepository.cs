using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IIncomeRepository
    {
        Task<IEnumerable<Income>> GetAllAsync();
        Task<Income> GetByIdAsync(int id);
        Task AddAsync(Income dto);
        Task DeleteAsync(int id);
        Task<Income> UpdateAsync(int id, Income dto);
    }
}
