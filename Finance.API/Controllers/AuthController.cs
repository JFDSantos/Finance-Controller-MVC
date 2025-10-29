using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Finance.Domain.Models;
using Finance.Application.Interfaces;
using Finance.Application.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Finance.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _user;

        public AuthController(IConfiguration config, IUserRepository user)
        {
            _config = config;
            _user = user;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            try
            {
                var user = await _user.ValidLoginUser(email, password);
                
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role, user.role.ToString()),
                    new Claim(ClaimTypes.Name, user.user)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                Response.Cookies.Append("jwt", tokenString, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)

                });

                return Ok("Login bem-sucedido");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
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
            try
            {
                var user = new User
                {
                    user = dto.user,
                    email = dto.email,
                    password = BCrypt.Net.BCrypt.HashPassword(dto.password)
                };

                await _user.AddAsync(user);
                return CreatedAtAction(nameof(Login), new { username = user.user }, user);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
 
        }

    }

}
