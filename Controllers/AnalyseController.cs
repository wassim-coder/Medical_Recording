using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medical.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.JsonPatch;
using medical.Data;

namespace MedicalApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AnalyseController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Analyse
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Analyse>>> GetAnalyses()
        {
            // Get the authenticated user's ID or email
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized("Invalid user token.");
                }
                var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
                if (userByEmail == null)
                {
                    return NotFound("User not found.");
                }
                userId = userByEmail.Id;
            }

            // Restrict access based on role or relationship
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            IQueryable<Analyse> query = _context.Analyses
                .Include(a => a.DossierMedical)
                .ThenInclude(d => d.Doctor)
                .Include(a => a.DossierMedical)
                .ThenInclude(d => d.Patient);

            if (user.Role == "Patient")
            {
                query = query.Where(a => a.DossierMedical.PatientId == userId);
            }
            else if (user.Role == "Doctor")
            {
                query = query.Where(a => a.DossierMedical.DoctorId == userId);
            }
            // Admins or other roles might see all analyses; adjust as needed

            return await query.ToListAsync();
        }

        // GET: api/Analyse/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Analyse>> GetAnalyse(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized("Invalid user token.");
                }
                var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
                if (userByEmail == null)
                {
                    return NotFound("User not found.");
                }
                userId = userByEmail.Id;
            }

            var analyse = await _context.Analyses
                .Include(a => a.DossierMedical)
                .ThenInclude(d => d.Doctor)
                .Include(a => a.DossierMedical)
                .ThenInclude(d => d.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (analyse == null)
            {
                return NotFound("Analyse not found.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Restrict access: only the associated doctor or patient can view
            if (user.Role == "Patient" && analyse.DossierMedical.PatientId != userId)
            {
                return Forbid("You are not authorized to view this analysis.");
            }
            if (user.Role == "Doctor" && analyse.DossierMedical.DoctorId != userId)
            {
                return Forbid("You are not authorized to view this analysis.");
            }

            return Ok(analyse);
        }

        // POST: api/Analyse
        [HttpPost]
        public async Task<ActionResult<Analyse>> PostAnalyse(Analyse analyse)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized("Invalid user token.");
                }
                var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
                if (userByEmail == null)
                {
                    return NotFound("User not found.");
                }
                userId = userByEmail.Id;
            }

            // Validate DossierMedical
            var dossier = await _context.DossiersMedical
                .Include(d => d.Doctor)
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == analyse.DossierMedicalId);

            if (dossier == null)
            {
                return BadRequest("Invalid or non-existent DossierMedical.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Only doctors can create analyses, or adjust based on your requirements
            if (user.Role != "Doctor")
            {
                return Forbid("Only doctors can create analyses.");
            }

            if (dossier.DoctorId != userId)
            {
                return Forbid("You are not authorized to create an analysis for this dossier.");
            }

            // Set navigation property to null to avoid deserialization issues
            analyse.DossierMedical = null;

            _context.Analyses.Add(analyse);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAnalyse), new { id = analyse.Id }, analyse);
        }

        // DELETE: api/Analyse/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnalyse(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized("Invalid user token.");
                }
                var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
                if (userByEmail == null)
                {
                    return NotFound("User not found.");
                }
                userId = userByEmail.Id;
            }

            var analyse = await _context.Analyses
                .Include(a => a.DossierMedical)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (analyse == null)
            {
                return NotFound("Analyse not found.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Only doctors can delete analyses, and only for their own dossiers
            if (user.Role != "Doctor")
            {
                return Forbid("Only doctors can delete analyses.");
            }

            if (analyse.DossierMedical.DoctorId != userId)
            {
                return Forbid("You are not authorized to delete this analysis.");
            }

            _context.Analyses.Remove(analyse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Analyse/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchAnalyse(int id, [FromBody] JsonPatchDocument<Analyse> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Patch document is null.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized("Invalid user token.");
                }
                var userByEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
                if (userByEmail == null)
                {
                    return NotFound("User not found.");
                }
                userId = userByEmail.Id;
            }

            var analyse = await _context.Analyses
                .Include(a => a.DossierMedical)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (analyse == null)
            {
                return NotFound("Analyse not found.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Only doctors can update analyses, and only for their own dossiers
            if (user.Role != "Doctor")
            {
                return Forbid("Only doctors can update analyses.");
            }

            if (analyse.DossierMedical.DoctorId != userId)
            {
                return Forbid("You are not authorized to update this analysis.");
            }

            // Prevent updates to sensitive or restricted fields
            foreach (var operation in patchDoc.Operations)
            {
                if (operation.path.ToLower().Contains("id") ||
                    operation.path.ToLower().Contains("dossiermedicalid") ||
                    operation.path.ToLower().Contains("dossiermedical"))
                {
                    return BadRequest($"Updating {operation.path} is not allowed.");
                }
            }

            // Apply the patch with custom error handling
            patchDoc.ApplyTo(analyse, error =>
            {
                ModelState.AddModelError(error.AffectedObject?.ToString() ?? "PatchError", error.ErrorMessage);
            });

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Error updating analyse: {ex.InnerException?.Message ?? ex.Message}");
            }

            return NoContent();
        }
    }
}