using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medical.Models
{
    public class Analyse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string AnalyseName { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(500)]
        public string ResultatAnalyse { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string Commentaire { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cout { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Remboursement { get; set; } = 0m;

        [Required]
        [ForeignKey("DossierMedical")]
        public int DossierMedicalId { get; set; }

        public DossierMedical DossierMedical { get; set; }
    }
}