using medical.Data;
using medical.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace medical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRendezVous([FromBody] RendezVousCreateDto dto)
        {
            try
            {
                // Validate patient exists and has role 'patient'
                var patient = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.PatientId && u.Role == "patient");
                if (patient == null)
                {
                    return BadRequest(new { Message = "Patient not found" });
                }

                // Validate doctor exists and has role 'doctor'
                var doctor = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == dto.DoctorId && u.Role == "doctor");
                if (doctor == null)
                {
                    return BadRequest(new { Message = "Doctor not found" });
                }

                // Check for existing rendez-vous with same doctor, date, time, and status 'approved'
                var rendezVousExists = await _context.Appointments
                    .AnyAsync(r => r.DoctorId == dto.DoctorId &&
                                   r.Date == dto.Date.Date &&
                                   r.Time == dto.Time &&
                                   r.Status == "approved");
                if (rendezVousExists)
                {
                    return BadRequest(new { Message = "Rendez-vous already scheduled" });
                }

                // Create new rendez-vous
                var rendezVous = new Appointment
                {
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    Date = dto.Date.Date, // Store only date
                    Time = dto.Time,
                    Status = "pending", // Default status
                    CreatedAt = DateTime.UtcNow
                };

                // Save to database
                _context.Appointments.Add(rendezVous);
                await _context.SaveChangesAsync();

                // Prepare response matching Express output
                var response = new
                {
                    Patient = rendezVous.PatientId,
                    Doctor = rendezVous.DoctorId,
                    Date = rendezVous.Date,
                    Time = rendezVous.Time
                };

                return StatusCode(201, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Server error", Error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserAppointments()
        {
            try
            {
                // Extract user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid user ID in token" });
                }

                // Fetch appointments where user is either Patient or Doctor
                var appointments = await _context.Appointments
                    .Where(r => r.PatientId == userId || r.DoctorId == userId)
                    .Include(r => r.Patient) // Include Patient details
                    .Include(r => r.Doctor)  // Include Doctor details
                    .Select(r => new
                    {
                        r.Id,
                        Patient = new { r.Patient.Id, r.Patient.Name, r.Patient.Email },
                        Doctor = new { r.Doctor.Id, r.Doctor.Name, r.Doctor.Email },
                        r.Date,
                        r.Time,
                        r.Status,
                        r.CreatedAt
                    })
                    .ToListAsync();

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Server error", Error = ex.Message });
            }
        }





    }

    public class RendezVousCreateDto
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
    }

}
