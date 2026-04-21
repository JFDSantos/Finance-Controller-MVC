using Finance.Domain.Models;
using Finance.Application.ViewModel;

namespace Finance.Application.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Category, int>
    {
    }
}
