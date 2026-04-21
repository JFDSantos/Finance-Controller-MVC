using Finance.Infrastructure.Data;
using Finance.Domain.Models;
using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Repositories
{
    public class IncomeRepository : BaseRepository<Income, int>, IIncomeRepository
    {
        public IncomeRepository(FinanceContext context) : base(context) { }
    }
}
