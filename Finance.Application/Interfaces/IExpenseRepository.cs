using Finance.Domain.Models;

namespace Finance.Application.Interfaces
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<Expense>> GetAllAsync();
        Task<Expense> GetByIdAsync(int id);
        Task AddAsync(Expense expense);
        Task DeleteAsync(int id);
        Task<Expense> UpdateAsync(int id, Expense expense);
    }
}
