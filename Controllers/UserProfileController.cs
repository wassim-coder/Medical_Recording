using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medical.Models;
using medical.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.JsonPatch;


namespace MedicalApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserProfile
        [HttpGet]
        public async Task<ActionResult<User>> GetProfile()
        {
            // Try to get user ID from NameIdentifier claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                // Fallback to email claim if NameIdentifier is not used
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized("Invalid user token.");
                }

                var userByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == emailClaim);

                if (userByEmail == null)
                {
                    return NotFound("User not found.");
                }

                userByEmail.Password = null;
                return Ok(userByEmail);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Password = null;
            return Ok(user);
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Users
                .Where(d => d.Role == "doctor")
                .Select(d => new {
                    d.Id,
                    d.Name,
                    d.Email
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var doctors = await _context.Users
                .Where(d => d.Role == "patient")
                .Select(d => new {
                    d.Id,
                    d.Name,
                    d.Email
                })
                .ToListAsync();

            return Ok(doctors);
        }


        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] User updatedUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the logged-in user's ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user token.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update fields manually (exclude Password)
            user.Name = updatedUser.Name;
            user.DateNaiss = updatedUser.DateNaiss;
            user.Genre = updatedUser.Genre;
            user.Role = updatedUser.Role;
            user.GroupeSanguin = updatedUser.GroupeSanguin;
            user.Specialite = updatedUser.Specialite;
            user.Salary = updatedUser.Salary;
            user.Allergies = updatedUser.Allergies;
            user.Code = updatedUser.Code;

            // Do not update: user.Password or user.Email (optional - can be blocked too)

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Failed to update user profile.");
            }

            user.Password = null; // Never return password
            return Ok(user);
        }


    }
}