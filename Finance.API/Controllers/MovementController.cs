using Finance.Application.Interfaces;
using Finance.Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MovementController : ControllerBase
    {
        private readonly IMovementQueryService _service;
        public MovementController(IMovementQueryService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? categoryId, [FromQuery] EMovimentType? type)
            => Ok(await _service.ListAsync(year, month, categoryId, type));
    }
}
