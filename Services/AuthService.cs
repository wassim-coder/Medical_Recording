using medical.Data;
using medical.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace medical.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<(string Token, object User)> Register(User user)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    throw new Exception("User already exists");
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.CreatedAt = DateTime.UtcNow;
                user.DateNaiss = user.DateNaiss == default ? DateTime.UtcNow : user.DateNaiss;
                user.Genre = user.Genre ?? "";
                user.Role = user.Role ?? "patient";

                if (user.Role == "doctor")
                {
                    user.Specialite = user.Specialite ?? "";
                    user.Salary = user.Salary;
                }
                else if (user.Role == "patient")
                {
                    user.GroupeSanguin = user.GroupeSanguin ?? "";
                    user.Allergies = user.Allergies ?? "";
                    user.Code = GenerateSecureRandomCode();
                }
                else
                {
                    user.Specialite = "";
                    user.Salary = 0;
                    user.GroupeSanguin = "";
                    user.Allergies = "";
                    user.Code = "";
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                var userResponse = new
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };

                return (token, userResponse);
            }
            catch (Exception ex)
            {
                throw new Exception($"Server error: {ex.Message}");
            }
        }

        public async Task<(string Token, object User)> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                throw new Exception("Invalid email or password.");
            }

            var token = GenerateJwtToken(user);

            var userResponse = new
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role
            };

            return (token, userResponse);
        }

        public async Task<string> RequestPasswordReset(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                // Return null if user doesn't exist (security best practice)
                return null;
            }

            // Generate reset token
            var token = Guid.NewGuid().ToString();

            // Store token in database
            var resetToken = new PasswordResetToken
            {
                Email = email,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsUsed = false
            };

            // Invalidate any existing tokens
            var existingTokens = await _context.PasswordResetTokens
                .Where(t => t.Email == email && !t.IsUsed)
                .ToListAsync();

            foreach (var t in existingTokens)
            {
                t.IsUsed = true;
            }

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            return token;
        }
        public async Task ResetPassword(string token, string newPassword)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null)
            {
                throw new Exception("Invalid or expired token.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetToken.Email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            resetToken.IsUsed = true;

            await _context.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "patient"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private string GenerateSecureRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new byte[6];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(random);
            return new string(Enumerable.Range(0, 6).Select(i => chars[random[i] % chars.Length]).ToArray());
        }

        private string GenerateResetToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber)
                .Replace("/", "")
                .Replace("+", "")
                .Replace("=", "");
        }

    }
}