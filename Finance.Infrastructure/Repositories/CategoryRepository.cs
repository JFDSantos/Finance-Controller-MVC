using Finance.Application.Interfaces;
using Finance.Domain.Models;
using Finance.Infrastructure.Data;

namespace Finance.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category, int>, ICategoryRepository
    {
        public CategoryRepository(FinanceContext context) : base(context)
        {
        }
    }

}