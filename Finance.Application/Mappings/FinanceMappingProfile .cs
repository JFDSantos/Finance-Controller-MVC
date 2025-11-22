using AutoMapper;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.Application.Mappings
{
    public class FinanceMappingProfile : Profile
    {
        public FinanceMappingProfile()
        {
            CreateMap<ExpenseCreateDto, Expense>();
            CreateMap<Expense, ExpenseDto>();
            CreateMap<IncomeCreateDto, Income>();
            CreateMap<Income, IncomeDto>();
            CreateMap<CategoryCreateDto, Category>();
            CreateMap<Category, CategorySelectDto>(); 
        }
    }
}
