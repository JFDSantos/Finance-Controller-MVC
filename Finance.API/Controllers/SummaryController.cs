using Finance.Application.Interfaces;
using Finance.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SummaryController : ControllerBase
    {
        private readonly ISummaryService _service;
        public SummaryController(ISummaryService service) => _service = service;

        // Nullable so a missing value reaches our validation (400 VALIDATION_FAILED) instead of the framework's.
        [HttpGet]
        public async Task<IActionResult> GetMonthly([FromQuery] int? year, [FromQuery] int? month)
        {
            PeriodValidation.ValidateYear(year);
            PeriodValidation.ValidateMonth(month);
            return Ok(await _service.GetMonthlyAsync(year!.Value, month!.Value));
        }

        [HttpGet("series")]
        public async Task<IActionResult> GetSeries([FromQuery] int months = 12)
            => Ok(await _service.GetSeriesAsync(months));
    }
}
