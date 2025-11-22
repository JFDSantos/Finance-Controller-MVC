using Finance.Application.ViewModel;
using Finance.Infrastructure.Data;
using Finance.Domain.Models;
using Finance.Domain.Models.Enums;
using Finance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finance.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _catService;
        public CategoryController(ICategoryService CatService)
        {
            _catService = CatService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _catService.GetAllAsync();
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
                var result = await _catService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto cat)
        {

            try
            {
                var created = await _catService.AddAsync(cat);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryCreateDto dto)
        {
            try
            {
                var updateCategory = await _catService.UpdateAsync(id, dto);
                return Ok(updateCategory);
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
                await _catService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
