using AutoMapper;
using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Services
{
    public class ExpenseService : BaseService<Expense, ExpenseCreateDto, ExpenseSelectDto>, IExpenseService
    {
        public ExpenseService(
            IBaseRepository<Expense, int> repository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<ExpenseCreateDto> validator) : base(repository, uow, mapper, validator)
        {
        }

    }
}
