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

        private const string NameTakenCode = "CATEGORY_NAME_TAKEN";
        private const string NameTakenMessage = "A category with this name already exists.";

        public override async Task<CategorySelectDto> AddAsync(CategoryCreateDto dto)
        {
            var clean = Trimmed(dto);
            await EnsureNameFreeAsync(clean.Name, null);
            try
            {
                return await base.AddAsync(clean);
            }
            catch (UniqueConstraintException)
            {
                throw new ConflictException(NameTakenCode, NameTakenMessage);
            }
        }

        public override async Task<CategorySelectDto> UpdateAsync(int id, CategoryCreateDto dto)
        {
            // Existence/ownership first: a category that is not the caller's is a 404, never a 409.
            _ = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Entity not found");

            var clean = Trimmed(dto);
            await EnsureNameFreeAsync(clean.Name, id);
            try
            {
                return await base.UpdateAsync(id, clean);
            }
            catch (UniqueConstraintException)
            {
                throw new ConflictException(NameTakenCode, NameTakenMessage);
            }
        }

        private static CategoryCreateDto Trimmed(CategoryCreateDto dto) => new() { Name = dto.Name?.Trim()! };

        // The repository is filtered by the current user, so this only sees the caller's categories.
        private async Task EnsureNameFreeAsync(string? name, int? ownId)
        {
            if (string.IsNullOrEmpty(name)) return; // the validator reports empty names
            var all = await _repository.GetAllAsync();
            if (all.Any(c => c.Id != ownId && string.Equals(c.Name?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException(NameTakenCode, NameTakenMessage);
        }

        public override async Task DeleteAsync(int id)
        {
            _ = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Entity not found");

            if (await _movements.AnyByCategoryAsync(id))
                throw new ConflictException("CATEGORY_IN_USE", "Category has movements and cannot be deleted");

            await base.DeleteAsync(id);
        }
    }
}
