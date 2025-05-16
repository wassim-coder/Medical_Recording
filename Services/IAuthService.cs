using medical.Models;
using System.Threading.Tasks;

namespace medical.Services
{
    public interface IAuthService
    {
        Task<(string Token, object User)> Register(User user);
        Task<(string Token, object User)> Login(string username, string password);
    }
}
