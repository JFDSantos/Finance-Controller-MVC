using Finance.Infrastructure.Data;
using Finance.Domain.Models;
using Finance.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Repositories
{
    public class ExpenseRepository : BaseRepository<Expense, int>, IExpenseRepository
    {
        public ExpenseRepository(FinanceContext context) : base(context)
        {
        }
    }
}
