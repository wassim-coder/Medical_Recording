using medical.Data;
using medical.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace medical.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(string Token, object User)> Register(User user)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    throw new Exception("User already exists");
                }

                // Hash the password
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

                // Set common default values
                user.CreatedAt = DateTime.UtcNow;
                user.DateNaiss = user.DateNaiss == default ? DateTime.UtcNow : user.DateNaiss;
                user.Genre = user.Genre ?? "";
                user.Role = user.Role ?? "patient"; // Default to patient if role is empty

                // Role-specific fields
                if (user.Role == "doctor")
                {
                    // Specialite and Salary are already part of User model
                    user.Specialite = user.Specialite ?? "";
                    user.Salary = user.Salary; // No change needed, defaults to 0
                }
                else if (user.Role == "patient")
                {
                    user.GroupeSanguin = user.GroupeSanguin ?? "";
                    user.Allergies = user.Allergies ?? "";
                    user.Code = GenerateSecureRandomCode(); // Generate random code
                }
                else
                {
                    // For other roles (e.g., admin), set only common fields
                    user.Specialite = "";
                    user.Salary = 0;
                    user.GroupeSanguin = "";
                    user.Allergies = "";
                    user.Code = "";
                }

                // Save user to PostgreSQL
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(user);

                // Prepare user response (matching Express output)
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
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateSecureRandomCode()
        {
            // Generate a 6-character random alphanumeric code
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new byte[6];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }
            var code = new char[6];
            for (int i = 0; i < 6; i++)
            {
                code[i] = chars[random[i] % chars.Length];
            }
            return new string(code);
        }
    }
}