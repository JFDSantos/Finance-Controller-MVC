using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseDto?> GetByIdAsync(int id);
        Task<IEnumerable<ExpenseDto>> GetAllAsync();
        Task<ExpenseDto> AddAsync(ExpenseCreateDto expenseDto);
        Task DeleteAsync(int id);
        Task<ExpenseDto> UpdateAsync(int id, ExpenseDto expenseDto);
    }
}
