namespace medical.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
