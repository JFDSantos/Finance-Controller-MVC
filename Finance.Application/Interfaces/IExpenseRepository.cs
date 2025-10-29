using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
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
