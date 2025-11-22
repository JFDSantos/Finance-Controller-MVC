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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryCreateDto> _validator;

        public CategoryService(
            ICategoryRepository repository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<CategoryCreateDto> validator)
        {
            _repository = repository;
            _uow = uow;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<CategorySelectDto> AddAsync(CategoryCreateDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
            
            var category = _mapper.Map<Category>(dto);
            await _repository.AddAsync(category);
            await _uow.CommitAsync();
            return _mapper.Map<CategorySelectDto>(category);
        }

        public async Task DeleteAsync(int id)
        {
            var existingCategory = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Category not found");

            await _repository.DeleteAsync(id);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<CategorySelectDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategorySelectDto>>(categories);
        }

        public async Task<CategorySelectDto> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            return _mapper.Map<CategorySelectDto>(category);
        }

        public async Task<CategorySelectDto> UpdateAsync(int id, CategoryCreateDto dto)
        {
            var existingCategory = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Category not found");

            var category = _mapper.Map<Category>(dto);

            await _repository.UpdateAsync(id, category);
            await _uow.CommitAsync();

            return _mapper.Map<CategorySelectDto>(category);
        }
    }
}
