using Finance.Web.Models;
using Finance.Web.ViewModel;

namespace Finance.Web.Patterns.Interfaces
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<ExpenseDto>> GetAllAsync();
        Task<IncomeDto> GetByIdAsync(int id);
        Task AddAsync(Income dto);
        Task DeleteAsync(int id);
        Task<IncomeDto> UpdateAsync(int id, IncomeCreateDto dto);
    }
}
