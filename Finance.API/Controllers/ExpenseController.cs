using Finance.Domain.Models;
using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseRepository _expService;
        public ExpenseController(IExpenseRepository expService)
        {
            _expService = expService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _expService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _expService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ExpenseCreateDto dto)
        {

            try
            {
                var expense = new Expense
                {
                    categoryId = dto.CategoryId,
                    description = dto.Description,
                    movimentDate = dto.MovimentDate,
                    isAppellant = dto.IsAppellant,
                    value = dto.Value
                };

                await _expService.AddAsync(expense);
                return CreatedAtAction(nameof(GetById), new { id = expense.id }, expense);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] ExpenseCreateDto dto)
        {
            try
            {
                var updateExpense = await _expService.UpdateAsync(id, dto);
                return Ok(updateExpense);
            }
            catch
            {
                return NotFound("Transaction not found!");
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _expService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
