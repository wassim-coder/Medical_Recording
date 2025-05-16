using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medical.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; } // Primary key, required by EF Core

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; } // Foreign key to User (Patient)
        public User Patient { get; set; } // Navigation property

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; } // Foreign key to User (Doctor)
        public User Doctor { get; set; } // Navigation property

        [Required]
        [Column(TypeName = "date")] // Stores only date (YYYY-MM-DD) in PostgreSQL
        public DateTime Date { get; set; } = DateTime.UtcNow.Date; // Default to current date

        [Required]
        [StringLength(5)] // e.g., "14:00"
        public string Time { get; set; } = ""; // Default to empty string

        [StringLength(10)] // Max length for "pending", "approved", "cancelled"
        public string Status { get; set; } = "pending"; // Default to "pending"

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default to current timestamp
    }
}