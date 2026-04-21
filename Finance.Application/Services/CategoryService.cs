using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using AutoMapper;
using FluentValidation;

namespace Finance.Application.Services
{
    public class CategoryService : BaseService<Category, CategoryCreateDto, CategorySelectDto>, ICategoryService
    {
        public CategoryService(
            IBaseRepository<Category, int> repository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<CategoryCreateDto> validator) 
            : base(repository, uow, mapper, validator)
        {
        }
    }
}
