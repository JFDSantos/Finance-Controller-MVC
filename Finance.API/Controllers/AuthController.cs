using Finance.Application.Interfaces;
using Finance.Application.Services;
using Finance.Application.ViewModel;
using Finance.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Finance.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IJWTService _jwtService;

        public AuthController(IUserService userService, IJWTService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost]
        public async Task<IActionResult?> Login([FromForm] string email, [FromForm] string password)
        {
            var user = await _userService.ValidLoginUserAsync(email, password);

            if (user == null)
                return BadRequest("Invalid email or password");

            // 2. Gera token via JwtService
            var token = _jwtService.GenerateToken(user);

            // 3. Setta cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok("Logout realizado");
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(UserCreateDto dto)
        {
            var createdUser = await _userService.AddAsync(dto); 

            return CreatedAtAction(nameof(Login), new { username = createdUser.user }, createdUser);
        }

    }

}
