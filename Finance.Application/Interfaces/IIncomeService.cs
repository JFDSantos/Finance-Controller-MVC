using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface IIncomeService : IBaseService<Income, IncomeCreateDto, IncomeSelectDto>
    {

    }
}
