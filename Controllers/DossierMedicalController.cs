using medical.Data;
using medical.Models;
using MedicalApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace medical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DossierMedicalController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DossierMedicalController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DossierMedical
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DossierMedical>>> GetDossiers()
        {
            return await _context.DossiersMedical
                .Include(d => d.Doctor)
                .Include(d => d.Patient)
                .ToListAsync();
        }

        // GET: api/DossierMedical/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DossierMedical>> GetDossier(int id)
        {
            var dossier = await _context.DossiersMedical
                .Include(d => d.Doctor)
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dossier == null)
            {
                return NotFound();
            }

            return dossier;
        }

        // POST: api/DossierMedical
        [HttpPost]
        public async Task<ActionResult<DossierMedical>> PostDossier(DossierMedicalDto dto)
        {
            // Validate Doctor and Patient roles
            var doctor = await _context.Users.FindAsync(dto.DoctorId);
            var patient = await _context.Users.FindAsync(dto.PatientId);

            if (doctor == null || doctor.Role != "doctor")
            {
                return BadRequest("Invalid or non-existent Doctor.");
            }

            if (patient == null || patient.Role != "patient")
            {
                return BadRequest("Invalid or non-existent Patient.");
            }

            var dossier = new DossierMedical
            {
                Name = dto.Name,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId
            };

            _context.DossiersMedical.Add(dossier);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDossier), new { id = dossier.Id }, dossier);
        }

        // PUT: api/DossierMedical/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDossier(int id, DossierMedicalDto dto)
        {
            var dossier = await _context.DossiersMedical.FindAsync(id);
            if (dossier == null)
            {
                return BadRequest();
            }

            // Validate Doctor and Patient roles
            var doctor = await _context.Users.FindAsync(dto.DoctorId);
            var patient = await _context.Users.FindAsync(dto.PatientId);

            if (doctor == null || doctor.Role != "doctor")
            {
                return BadRequest("Invalid or non-existent Doctor.");
            }

            if (patient == null || patient.Role != "patient")
            {
                return BadRequest("Invalid or non-existent Patient.");
            }

            dossier.Name = dto.Name;
            dossier.DoctorId = dto.DoctorId;
            dossier.PatientId = dto.PatientId;

            _context.Entry(dossier).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DossierExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/DossierMedical/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDossier(int id)
        {
            var dossier = await _context.DossiersMedical.FindAsync(id);
            if (dossier == null)
            {
                return NotFound();
            }

            _context.DossiersMedical.Remove(dossier);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DossierExists(int id)
        {
            return _context.DossiersMedical.Any(e => e.Id == id);
        }

    }
}
