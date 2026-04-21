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
    public class IncomeService : BaseService<Income, IncomeCreateDto, IncomeSelectDto>, IIncomeService
    {
        public IncomeService(
            IBaseRepository<Income, int> repository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<IncomeCreateDto> validator) : base(repository, uow, mapper, validator)
        {
        }
    }
}
