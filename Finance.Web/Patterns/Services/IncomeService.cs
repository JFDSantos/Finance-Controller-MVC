using Finance.Web.Models;
using Finance.Web.Patterns.Interfaces;
using Finance.Web.ViewModel;

namespace Finance.Web.Patterns.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly IIncomeRepository _repository;

        public IncomeService(IIncomeRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(IncomeCreateDto dto)
        {
            var income = new Income
            {
                categoryId = dto.CategoryId,
                description = dto.Description,
                isAppellant = dto.IsAppellant,
                value = dto.Value,
                movimentDate = dto.MovimentDate
            };

            await _repository.AddAsync(income);

        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<IncomeDto>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IncomeDto> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IncomeDto> UpdateAsync(int id, IncomeCreateDto dto)
        {
            return await _repository.UpdateAsync(id, dto);
        }
    }
}
