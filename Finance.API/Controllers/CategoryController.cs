using Finance.Application.ViewModel;
using Finance.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var result = await _catService.GetAllAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _catService.GetByIdAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto cat)
        {
            var created = await _catService.AddAsync(cat);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryCreateDto dto)
        {
            var updateCategory = await _catService.UpdateAsync(id, dto);
            return Ok(updateCategory);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            await _catService.DeleteAsync(id);
            return NoContent();
        }
    }
}
