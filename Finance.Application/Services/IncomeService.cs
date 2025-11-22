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
    public class IncomeService : IIncomeService
    {
        private readonly IIncomeRepository _repository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<IncomeCreateDto> _validator;

        public IncomeService(
            IIncomeRepository repository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<IncomeCreateDto> validator)
        {
            _repository = repository;
            _uow = uow;
            _mapper = mapper;
            _validator = validator;
        }
        public async Task<IncomeDto> AddAsync(IncomeCreateDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var income = _mapper.Map<Income>(dto);

            await _repository.AddAsync(income);
            await _uow.CommitAsync();

            return _mapper.Map<IncomeDto>(income);
        }

        public async Task DeleteAsync(int id)
        {
            var income = _repository.GetByIdAsync(id) ?? throw new NotImplementedException("Income not found");

            await _repository.DeleteAsync(id);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<IncomeDto>> GetAllAsync()
        {
            var incomes = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<IncomeDto>>(incomes);
        }

        public async Task<IncomeDto> GetByIdAsync(int id)
        {
            var income = await _repository.GetByIdAsync(id) ?? throw new NotImplementedException("Income not found");
            return _mapper.Map<IncomeDto>(income);
        }

        public async Task<IncomeDto> UpdateAsync(int id, IncomeCreateDto dto)
        {
            var incomeExist = _repository.GetByIdAsync(id) ?? throw new NotImplementedException("Income not found");

            var income = _mapper.Map<Income>(dto);

            await _repository.UpdateAsync(id, income);

            await _uow.CommitAsync();

            return _mapper.Map<IncomeDto>(income);

        }
    }
}
