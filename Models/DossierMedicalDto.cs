using System.ComponentModel.DataAnnotations;

namespace MedicalApp.Models
{
    public class DossierMedicalDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }
    }
}