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
    public abstract class BaseService<TEntity, TCreateDto, TSelectDto> : IBaseService<TEntity, TCreateDto, TSelectDto>
        where TEntity : class
    {
        protected IBaseRepository<TEntity, int> _repository;
        protected IUnitOfWork _uow;
        protected IMapper _mapper;
        protected IValidator<TCreateDto> _validator;

        public BaseService(
           IBaseRepository<TEntity, int> repository,
           IUnitOfWork uow,
           IMapper mapper,
           IValidator<TCreateDto> validator)
        {
            _repository = repository;
            _uow = uow;
            _mapper = mapper;
            _validator = validator;
        }

        public virtual async Task<TSelectDto> AddAsync(TCreateDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var entity = _mapper.Map<TEntity>(dto);
            await _repository.AddAsync(entity);
            await _uow.CommitAsync();
            return _mapper.Map<TSelectDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var existingEntity = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Entity not found");

            await _repository.DeleteAsync(id);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<TSelectDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync() ?? throw new KeyNotFoundException("Entities not found");
            return _mapper.Map<IEnumerable<TSelectDto>>(entities);
        }

        public async Task<TSelectDto> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Entity not found");
            return _mapper.Map<TSelectDto>(entity);
        }

        public async Task<TSelectDto> UpdateAsync(int id, TCreateDto dto)
        {
            var existingEntity = await _repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Entity not found");

            var entity = _mapper.Map<TEntity>(dto);

            await _repository.UpdateAsync(id, entity);
            await _uow.CommitAsync();

            return _mapper.Map<TSelectDto>(entity);
        }
    }
}
