using medical.Models;
using medical.Services;
using Microsoft.AspNetCore.Mvc;

namespace medical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            try
            {
                var (token, userResponse) = await _authService.Register(user);
                return StatusCode(201, new { Token = token, User = userResponse });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (token, user) = await _authService.Login(request.Email, request.Password);
                return Ok(new
                {
                    Token = token,
                    Email = ((dynamic)user).Email,
                    Role = ((dynamic)user).Role
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }



    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}