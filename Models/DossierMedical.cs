using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace medical.Models
{
    public class DossierMedical
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public User Doctor { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public User Patient { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
