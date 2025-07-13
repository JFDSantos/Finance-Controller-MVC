using Finance.Web.Models;
using Finance.Web.ViewModel;

namespace Finance.Web.Patterns.Interfaces
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<ExpenseDto>> GetAllAsync();
        Task<ExpenseDto> GetByIdAsync(int id);
        Task AddAsync(Expense dto);
        Task DeleteAsync(int id);
        Task<ExpenseDto> UpdateAsync(int id, ExpenseCreateDto dto);
    }
}
