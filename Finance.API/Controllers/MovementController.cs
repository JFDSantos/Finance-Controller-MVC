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
        {
            // Model binding accepts any integer for an enum (type=5); reject undefined values.
            if (type is not null && !Enum.IsDefined(type.Value))
                throw new FluentValidation.ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("type", "The value is not a valid movement type.") { ErrorCode = "INVALID_VALUE" }
                });
            return Ok(await _service.ListAsync(year, month, categoryId, type));
        }
    }
}
