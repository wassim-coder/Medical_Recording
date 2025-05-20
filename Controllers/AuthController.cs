using medical.Models;
using medical.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace medical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService; // Added
        private readonly IConfiguration _configuration; // Added

        public AuthController(
            IAuthService authService,
            IEmailService emailService, // Added
            IConfiguration configuration) // Added
        {
            _authService = authService;
            _emailService = emailService;
            _configuration = configuration;
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

                // Explicitly cast 'user' to its expected type to access the 'Email' and 'Role' properties
                var typedUser = user as User; // Replace 'User' with the actual type of 'user' if different

                if (typedUser == null)
                {
                    return BadRequest(new { Message = "Invalid user data." });
                }

                return Ok(new
                {
                    Token = token,
                    Email = typedUser.Email,
                    Role = typedUser.Role
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                await _emailService.SendPasswordResetEmail(
                    "test@example.com",
                    "https://example.com/reset-password?token=test123");
                return Ok("Test email sent successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to send test email: {ex.Message}");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var token = await _authService.RequestPasswordReset(request.Email);

                if (token != null)
                {
                    var resetLink = $"{_configuration["Frontend:BaseUrl"]}/reset-password?token={token}";
                    await _emailService.SendPasswordResetEmail(request.Email, resetLink);
                }

                // Always return success to prevent email enumeration
                return Ok(new { Message = "If an account exists, a reset link has been sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                await _authService.ResetPassword(request.Token, request.NewPassword);
                return Ok(new { Message = "Password has been reset successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }

    // Request models moved to separate files in a "Models/Requests" folder would be better
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}