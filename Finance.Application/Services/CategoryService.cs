using Finance.Application.Exceptions;
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
            IValidator<CategoryCreateDto> validator,
            IMovementReadRepository movements)
            : base(repository, uow, mapper, validator)
        {
            _movements = movements;
        }

        private readonly IMovementReadRepository _movements;

        public override async Task DeleteAsync(int id)
        {
            _ = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Entity not found");

            if (await _movements.AnyByCategoryAsync(id))
                throw new ConflictException("CATEGORY_IN_USE", "Category has movements and cannot be deleted");

            await base.DeleteAsync(id);
        }
    }
}
