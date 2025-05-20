using System.Threading.Tasks;

namespace medical.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmail(string email, string resetLink);
    }
}