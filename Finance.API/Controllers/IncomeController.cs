using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Finance.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IncomeController : ControllerBase
    {
        private readonly IIncomeService _IncomeService;
        public IncomeController(IIncomeService service)
        {
            _IncomeService = service;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var incomes = await _IncomeService.GetAllAsync();
            return Ok(incomes);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var income = await _IncomeService.GetByIdAsync(id);
            return Ok(income);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(IncomeCreateDto dto)
        {
            var created = await _IncomeService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id}, created);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _IncomeService.DeleteAsync(id);
            return NoContent();
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] IncomeCreateDto dto)
        {
            var income = await _IncomeService.UpdateAsync(id,dto);
            return Ok(income);
        }
    }
}
