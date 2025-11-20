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
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<ExpenseCreateDto> _validator;

        public ExpenseService(
            IExpenseRepository repository,
            IUnitOfWork uow,
            IMapper mapper,
            IValidator<ExpenseCreateDto> validator)
        {
            _repository = repository;
            _uow = uow;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<IEnumerable<ExpenseDto>> GetAllAsync()
        {
            var expenses = await _repository.GetAllAsync();

            return _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
        }

        public async Task<ExpenseDto?> GetByIdAsync(int id)
        {
            var expense = await _repository.GetByIdAsync(id);

            return _mapper.Map<ExpenseDto>(expense);
        }

        public async Task<ExpenseDto> AddAsync(ExpenseCreateDto expenseDto)
        {
            var validation = await _validator.ValidateAsync(expenseDto);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var expense = _mapper.Map<Expense>(expenseDto);

            await _repository.AddAsync(expense);
            await _uow.CommitAsync();

            return _mapper.Map<ExpenseDto>(expense);
        }
 
        public async Task<ExpenseDto> UpdateAsync(int id, ExpenseDto expenseDto)
        {
            var existingExpense = await _repository.GetByIdAsync(id) ?? throw new NotImplementedException("Expense not found");

            var expense = _mapper.Map<Expense>(expenseDto);

            await _repository.UpdateAsync(id,expense);

            await _uow.CommitAsync();

            return _mapper.Map<ExpenseDto>(expense);
        }
        public async Task DeleteAsync(int id)
        {
            var existingExpense = await _repository.GetByIdAsync(id) ?? throw new NotImplementedException("Expense not found");

            await _repository.DeleteAsync(id);

            await _uow.CommitAsync();
        }

    }
}
