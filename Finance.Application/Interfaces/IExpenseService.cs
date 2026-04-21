using Finance.Application.ViewModel;
using Finance.Domain.Models;

namespace Finance.Application.Interfaces
{
    public interface IExpenseService : IBaseService<Expense, ExpenseCreateDto, ExpenseSelectDto>
    {

    }
}
