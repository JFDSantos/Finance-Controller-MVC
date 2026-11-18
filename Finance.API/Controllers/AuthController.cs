using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Finance.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string CookieName = "jwt";
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

        private readonly IUserService _userService;
        private readonly IJWTService _jwtService;
        private readonly ICurrentUser _currentUser;
        private readonly IValidator<LoginDto> _loginValidator;

        public AuthController(IUserService userService, IJWTService jwtService, ICurrentUser currentUser, IValidator<LoginDto> loginValidator)
        {
            _userService = userService;
            _jwtService = jwtService;
            _currentUser = currentUser;
            _loginValidator = loginValidator;
        }

        private static CookieOptions CookieFlags() => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
        };

        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("register")]
        public async Task<IActionResult> Register(UserCreateDto dto)
        {
            var created = await _userService.RegisterAsync(dto);
            return Created("/api/Auth/me", created);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto? dto)
        {
            dto ??= new LoginDto();
            await _loginValidator.ValidateAndThrowAsync(dto);

            var user = await _userService.ValidLoginUserAsync(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(new { code = "INVALID_CREDENTIALS", message = "Invalid email or password" });

            var options = CookieFlags();
            options.Expires = DateTimeOffset.UtcNow.Add(TokenLifetime);
            Response.Cookies.Append(CookieName, _jwtService.GenerateToken(user), options);

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(CookieName, CookieFlags());
            return Ok(new { message = "Logout successful" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            try
            {
                return Ok(await _userService.GetByIdAsync(_currentUser.UserId));
            }
            catch (KeyNotFoundException)
            {
                // Valid token for an account that no longer exists.
                throw new UnauthorizedAccessException("Account no longer exists");
            }
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMe()
        {
            try
            {
                await _userService.DeleteAccountAsync(_currentUser.UserId);
            }
            catch (KeyNotFoundException)
            {
                throw new UnauthorizedAccessException("Account no longer exists");
            }
            Response.Cookies.Delete(CookieName, CookieFlags());
            return NoContent();
        }
    }
}
