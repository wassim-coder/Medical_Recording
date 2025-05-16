using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medical.Models
{
    public class User
    {
        [Key] 
        public int Id { get; set; } 

        [StringLength(100)]
        public string Name { get; set; } = ""; 

        [Required]
        [StringLength(255)] 
        [EmailAddress] 
        public string Email { get; set; } 

        [Required]
        [MinLength(6)] 
        public string Password { get; set; } 

        [Column(TypeName = "date")] 
        public DateTime DateNaiss { get; set; } = DateTime.UtcNow; 

        [StringLength(6)] 
        public string Genre { get; set; } = "";

        [StringLength(10)]
        public string Role { get; set; } 

        [StringLength(3)] 
        public string? GroupeSanguin { get; set; } = "";

        [StringLength(100)]
        public string? Specialite { get; set; } = "";

        [Column(TypeName = "decimal(18,2)")] 
        public decimal Salary { get; set; } = 0m; 

        public string? Allergies { get; set; } = "";

        [StringLength(50)] 
        public string? Code { get; set; } = "";

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}