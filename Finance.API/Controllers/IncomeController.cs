using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Finance.Web.Data;
using Finance.Web.Models;
using Finance.Web.Models.Enums;
using Finance.Web.Patterns.Interfaces;
using Finance.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


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
        public async Task<ActionResult<Income>> GetAll()
        {
            try
            {
                var incomes = await _IncomeService.GetAllAsync();
                return Ok(incomes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Income>> GetById(int id)
        {
            try
            {
                var income = await _IncomeService.GetByIdAsync(id);
                return Ok(income);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Income>> Create(IncomeCreateDto dto)
        {

            try
            {
                await _IncomeService.AddAsync(dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult<Income>> Delete(int id)
        {
            try
            {
                await _IncomeService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<ActionResult<Income>> Update(int id, [FromBody] IncomeCreateDto dto)
        {
            try
            {
                var income = await _IncomeService.UpdateAsync(id,dto);
                return CreatedAtAction(nameof(GetById), new { id = income.Id }, income);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
