using Finance.Domain.Models;

namespace Finance.Application.Interfaces
{
    public interface IExpenseRepository : IBaseRepository<Expense, int>
    {
    }
}
