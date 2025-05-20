using System.ComponentModel.DataAnnotations;

namespace medical.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
